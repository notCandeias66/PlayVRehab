from flask import Flask, request, render_template, redirect, flash, session
import sqlite3
from flask_bcrypt import Bcrypt

import qrcode
import base64
from io import BytesIO
from flask import session, jsonify
import re

app = Flask(__name__)
app.secret_key = 'your_secret_key'  # Needed for session management (flashing messages)
bcrypt = Bcrypt(app)

# Path to your SQLite database
DATABASE = 'PlayVRehab.db'

VALID_CODES = {
    "!", "CL", "CM", "CR", "JL", "JM", "JR", "GND", "BS",
    # Time-related codes pattern prefix:
    "LHLL", "LHLM", "LHLR", "LHRL", "LHRM", "LHRR"
}

# Help functions

def calculate_progress(progression_str):
    if not progression_str:
        return 0  # no data, no progress
    
    parts = progression_str.split(',')
    try:
        idx = parts.index('!')
        total = len(parts) - 1
        if total <= 0:
            return 0
        progress = (idx / total) * 100
        return round(progress, 2)
    except ValueError:
        # "!" not found
        return 0
    
def is_valid_level_string(level_string):
    parts = level_string.split(',')

    # First code must be "!"
    if not parts or parts[0] != "!":
        return False

    for code in parts:
        if code in VALID_CODES:
            continue

        # Check for time-based move: e.g., LHLL3
        match = re.fullmatch(r"(LHLL|LHLM|LHLR|LHRL|LHRM|LHRR)(\d+)", code)
        if match:
            continue

        return False  # If not in allowed or matching the time pattern
    return True


# Route functions

# Connect to SQLite database
def get_db_connection():
    conn = sqlite3.connect(DATABASE)
    conn.row_factory = sqlite3.Row
    return conn


# Route for displaying the index page (home page)
@app.route('/')
def index():
    return render_template('index.html')  # Optionally keep the index page


# Route for handling login
@app.route('/login', methods=['GET', 'POST'])
def login():
    if request.method == 'POST':
        email = request.form['email']
        password = request.form['password']

        # Fetch the user from the database
        conn = get_db_connection()
        user = conn.execute('SELECT * FROM "User" WHERE email = ?', (email,)).fetchone()
        conn.close()

        # Check if user exists and verify password
        if user and bcrypt.check_password_hash(user['password_hash'], password):
            # Store user details in session
            session['user_id'] = user['id']
            session['role'] = user['role']  # Store the user's role in session
            flash('Login successful!', 'success')
            return redirect('/homepage')  # Redirect to the homepage on successful login
        else:
            flash('Invalid email or password.', 'error')

    return render_template('login.html')  # Render login.html for GET requests


# Route for handling logout
@app.route('/logout')
def logout():
    session.clear()  # Clear session data
    flash('You have been logged out.', 'success')
    return redirect('/')


# Route for displaying the sign-up page (only accessible by admins or physio)
@app.route('/sign_up', methods=['GET', 'POST'])
def sign_up():
    # Ensure only admin or physio can access the sign-up page
    if 'role' not in session or session['role'] not in ['admin', 'physio']:
        flash('Access denied. Only admins and physios can create users.', 'error')
        return redirect('/')  # Redirect to the index page if unauthorized

    conn = get_db_connection()

    if request.method == 'POST':
        name = request.form['name']
        email = request.form['email']
        password = request.form['password']
        role = request.form.get('role')  # Get role from the form (admins choose this)

        # Hash the password
        password_hash = bcrypt.generate_password_hash(password).decode('utf-8')

        # Check if the email already exists
        existing_user = conn.execute('SELECT * FROM "User" WHERE email = ?', (email,)).fetchone()

        if existing_user:
            flash('Email address already exists. Please use a different email.', 'error')
            conn.close()
            return render_template('sign_up.html', role=session['role'], physios=[])

        # Prepare to insert the physio ID if the user is a patient
        physio_id = None

        if session['role'] == 'physio':  # Physio is creating a patient
            role = 'patient'  # Physio can only create patients

            # Get the selected physio from the form (optional)
            assigned_physio = request.form.get('physio')

            if assigned_physio:  # If a physio is selected from the dropdown
                physio_id = assigned_physio
            else:
                physio_id = session['user_id']  # Assign the current physio if none is selected

        elif session['role'] == 'admin' and role == 'patient':  # Admin is creating a patient
            # Admin can assign a physio for the patient
            physio_id = request.form.get('physio')  # Get physio ID from form if selected

        weight = None
        height = None
        bmi = None
        bmi_class = None

        if role == 'patient':
            # Get weight and height from form
            weight_kg = request.form.get('weight')
            height_m = request.form.get('height')

            if weight_kg and height_m:
                try:
                    # Convert to float for calculation
                    weight_float = float(weight_kg)
                    height_float = float(height_m)

                    # Store as integers (multiplied by 10 for precision)
                    weight = int(weight_float * 10)  # Store 75.24 kg as 752
                    height = int(height_float * 100) # Store 1.973 m as 197 (cm)

                    # Calculate BMI
                    bmi_float = weight_float / (height_float * height_float)

                    # Round to one decimal place first, then multiply by 10
                    bmi_rounded = round(bmi_float, 1)  # 22.9 stays 22.9
                    bmi = int(bmi_rounded * 10)  # Convert to 229

                    # Determine BMI category
                    if bmi_float < 18.5:
                        bmi_class = 'Underweight'
                    elif 18.5 <= bmi_float < 25:
                        bmi_class = 'Healthy'
                    elif 25 <= bmi_float < 30:
                        bmi_class = 'Overweight'
                    else:
                        bmi_class = 'Obese'

                except (ValueError, TypeError):
                    flash('Invalid height or weight values.', 'error')
                    #conn.close()
                    physios = conn.execute('SELECT id, name FROM "User" WHERE role = ?', ('physio',)).fetchall()
                    return render_template('sign_up.html', role=session['role'], physios=physios)

        # Insert user data into the SQLite database with the specified role and optional physio ID
        conn.execute('''INSERT INTO "User" (name, email, role, weightX10, heightX100, bmiX10, bmi_class, password_hash, physio_id) 
                        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)''',
                     (name, email, role, weight, height, bmi, bmi_class, password_hash, physio_id))
        conn.commit()
        conn.close()

        flash('User created successfully!', 'success')
        return redirect('/homepage')  # Redirect to the homepage after user creation

    # Fetch physios for the dropdown (users with role 'physio')
    physios = conn.execute('SELECT id, name FROM "User" WHERE role = ?', ('physio',)).fetchall()
    conn.close()

    return render_template('sign_up.html', role=session['role'], physios=physios)


# Route for displaying the homepage based on role
@app.route('/homepage', methods=['GET', 'POST'])
def homepage():
    if 'user_id' not in session:
        flash("Please log in first!", "error")
        return redirect('/login')

    conn = get_db_connection()
    role = session.get('role')
    user_id = session.get('user_id')

    # Fetch user details
    user = conn.execute('SELECT * FROM "User" WHERE id = ?', (user_id,)).fetchone()

    # Ensure user exists
    if not user:
        flash("User not found!", "error")
        conn.close()
        return redirect('/logout')

    user = dict(user)  # Convert to dictionary

    # Fetch patients and physios based on role
    physios = []
    patients = []
    levels = []

    if role in ['admin', 'physio']:
        levels = conn.execute('SELECT id, level_string, description FROM Level').fetchall()

    if role == 'admin':
        physios = conn.execute('SELECT * FROM "User" WHERE role = ?', ('physio',)).fetchall()
        patients = conn.execute('SELECT * FROM "User" WHERE role = ?', ('patient',)).fetchall()
    elif role == 'physio':
        patients = conn.execute('SELECT * FROM "User" WHERE role = ? AND physio_id = ?', ('patient', user_id)).fetchall()
    
    # Handle POST request for QR code generation
    qr_code = None
    userLevels = None

    if role == 'patient':
        if request.method == 'POST':
            data = request.get_json()  # Get data sent from JS
            patient_id = data.get('patient_id')

            if patient_id:
                # Fetch patient info
                patient = conn.execute('SELECT id, name, password_hash, qrcode_number FROM "User" WHERE id = ?', (patient_id)).fetchone()

                if patient:
                    # Increment qrcode_number
                    new_qrcode_number = patient['qrcode_number'] + 1

                    conn.execute('UPDATE "User" SET qrcode_number = ? WHERE id = ?', (new_qrcode_number, patient_id))
                    conn.commit()

                    # QR code content
                    qr_data = f"{patient['id']};{patient['name']};{patient['password_hash']};{new_qrcode_number}"

                    # Generate QR code
                    qr = qrcode.make(qr_data)
                    buffer = BytesIO()
                    qr.save(buffer, format="PNG")
                    qr_base64 = base64.b64encode(buffer.getvalue()).decode('utf-8')

                    # Return the QR code as base64
                    return jsonify({"qr_code": qr_base64})
            
        # Fetch patient levels if the user is a patient
        userLevels = conn.execute('''
            SELECT ul.*, l.level_string
            FROM "UserLevels" ul
            LEFT JOIN "Level" l ON ul.level_id = l.id
            WHERE ul.user_id = ?
        ''', (user_id,)).fetchall()

        userLevels = [dict(ul) for ul in userLevels]
        for ul in userLevels:
            prog_str = ul.get('progression')
            if not prog_str:
                prog_str = ul.get('level_string')
            ul['progress_percent'] = calculate_progress(prog_str)
    else:
        userLevels = None

    conn.close()

    # For GET request, render the homepage template
    return render_template('homepage.html', patients=patients, physios=physios, levels=levels, role=role, user=user, qr_code=qr_code, userLevels=userLevels)


# Route for viewing a specific physio
@app.route('/physio_detail/<int:physio_id>')
def physio_detail(physio_id):
    conn = get_db_connection()

    role = session.get('role')
    
    # Fetch the physio by ID
    physio = conn.execute('SELECT * FROM "User" WHERE id = ? AND role = ?', (physio_id, 'physio')).fetchone()

    patients = conn.execute('SELECT name, email, weightX10, heightX100, bmiX10, bmi_class FROM "User" WHERE physio_id = ? AND role = ?', (physio_id, 'patient')).fetchall()

    conn.close()
    
    if physio is None:
        flash('Physio not found.', 'error')
        return redirect('/homepage')
    
    return render_template('physio_detail.html', physio=physio, patients=patients, role=role)


# Route for viewing a specific patient
@app.route('/patient_detail/<int:patient_id>')
def patient_detail(patient_id):
    conn = get_db_connection()
    
    # Fetch the patient by ID
    patient = conn.execute('SELECT * FROM "User" WHERE id = ? AND role = ?', (patient_id, 'patient')).fetchone()

    if patient is None:
        flash('Patient not found.', 'error')
        return redirect('/homepage')
    
    # Fetch levels associated with this patient
    userLevels = conn.execute('''
        SELECT ul.*, l.level_string 
        FROM "UserLevels" ul
        LEFT JOIN "Level" l ON ul.level_id = l.id
        WHERE ul.user_id = ?
    ''', (patient_id,)).fetchall()

    # Convert results BEFORE closing the connection
    patient = dict(patient)  # Convert SQLite Row to dictionary
    userLevels = [dict(userLevel) for userLevel in userLevels]  # Convert each row to dictionary

    for ul in userLevels:
        prog_str = ul.get('progression')
        if not prog_str:  # empty or None
            prog_str = ul.get('level_string')
        ul['progress_percent'] = calculate_progress(prog_str)

    conn.close()
    
    return render_template('patient_detail.html', patient=patient, userLevels=userLevels)


# Route for creating a new User level
@app.route('/create_userLevel', methods=['GET', 'POST'])
def create_userLevel():
    if 'user_id' not in session:
        flash("Please log in first!", "error")
        return redirect('/login')

    conn = get_db_connection()
    role = session.get('role')
    user_id = session.get('user_id')

    if role == 'admin':
        patients = conn.execute('SELECT id, name FROM "User" WHERE role = "patient"').fetchall()
    elif role == 'physio':
        patients = conn.execute('SELECT id, name FROM "User" WHERE role = "patient" AND physio_id = ?', (user_id,)).fetchall()
    else:
        flash("Unauthorized access!", "error")
        conn.close()
        return redirect('/homepage')
    
    levels = conn.execute('SELECT id, description FROM "Level"').fetchall()

    if request.method == 'POST':
        patient_id = request.form.get('patient_id')
        level_id = request.form.get('level_id')

        if not patient_id or not level_id:
            flash("Please fill out all fields.", "error")
            return redirect('/create_userLevel')
        
        #level_string = conn.execute('SELECT level_string FROM "Level" WHERE id = ?', (level_id))

        # Fetch level_string from the Level table
        level_string_row = conn.execute('SELECT level_string FROM "Level" WHERE id = ?', (level_id,)).fetchone()
        if level_string_row is None:
            flash("Selected level not found.", "error")
            conn.close()
            return redirect('/create_userLevel')
        
        level_string = level_string_row['level_string']

        last_level_attempt = conn.execute('SELECT MAX(attempt) FROM "UserLevels" WHERE user_id = ? and level_id = ?', (patient_id, level_id,)).fetchone()
        next_level_attempt = (last_level_attempt[0] or 0) + 1

        conn.execute('INSERT INTO "UserLevels" (user_id, level_id, attempt, progression) VALUES (?, ?, ?, ?)',
                    (patient_id, level_id, next_level_attempt, level_string))
        conn.commit()
        conn.close()

        flash("UserLevel created successfully!", "success")
        return redirect('/homepage')

    conn.close()
    return render_template('create_userLevel.html', patients=patients, levels=levels)

@app.route('/new_level', methods=['GET', 'POST'])
def new_level():
    conn = get_db_connection()

    if request.method == 'POST':
        level_string = request.form['level_string'].strip()
        level_description = request.form['level_description'].strip()

        if not is_valid_level_string(level_string):
            conn.close()
            flash("Invalid level string. Make sure it starts with '!' and contains only valid codes.", "error")
            return render_template('new_level.html')

         # Get current max id from Level table
        cur = conn.execute('SELECT MAX(id) AS max_id FROM Level')
        row = cur.fetchone()
        next_id = (row['max_id'] or 0) + 1  # Handle case when table is empty

        # Insert new level
        conn.execute(
            'INSERT INTO Level (id, level_string, description) VALUES (?, ?, ?)',
            (next_id, level_string, level_description)
        )
        conn.commit()
        conn.close()

        return redirect('/homepage')  # Or redirect to a specific page

    conn.close()

    return render_template('new_level.html')

# Route for edtiting user level observations
@app.route('/edit_observations/<int:user_id>/<int:level_id>/<int:attempt>', methods=['GET', 'POST'])
def edit_observations(user_id, level_id, attempt):
    conn = get_db_connection()

    # Fetch the user level record
    user_level = conn.execute(
        'SELECT * FROM "UserLevels" WHERE user_id = ? AND level_id = ? AND attempt = ?', (user_id, level_id, attempt)
    ).fetchone()

    if user_level is None:
        conn.close()
        flash('User Level not found.', 'error')
        return redirect('/homepage')

    if request.method == 'POST':
        new_observations = request.form['observations']

        # Update the observations field
        conn.execute(
            'UPDATE "UserLevels" SET observations = ? WHERE user_id = ? AND level_id = ? AND attempt = ?',
            (new_observations, user_id, level_id, attempt)
        )
        conn.commit()

        # Redirect to the patient's detail page
        conn.close()
        return redirect(f'/patient_detail/{user_id}')

    conn.close()
    return render_template('edit_observations.html', user_level=user_level)


if __name__ == '__main__':
    app.run(debug=True)

