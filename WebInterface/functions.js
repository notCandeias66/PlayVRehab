// Handle role selection and show/hide physio selection
function handleRoleChange() {
    const role = document.getElementById("role").value;
    const physioSelection = document.getElementById("physio-selection");
    const patientFields = document.getElementById("patient-fields");
    const physioSelect = document.getElementById("physio");

    // Show the physio dropdown and patient fields if 'Patient' is selected
    if (role === "patient") {
        if (physioSelection) {
            physioSelection.style.display = "block";
            if (physioSelect) {
                physioSelect.required = true;
            }
        }
        if (patientFields) {
            patientFields.style.display = "block";
        }
    } else {
        if (physioSelection) {
            physioSelection.style.display = "none";
            if (physioSelect) {
                physioSelect.required = false;
            }
        }
        if (patientFields) {
            patientFields.style.display = "none";
            // Clear patient field values when hiding
            const heightInput = document.getElementById('height');
            const weightInput = document.getElementById('weight');
            if (heightInput) heightInput.value = '';
            if (weightInput) weightInput.value = '';
            calculateBMI(); // Reset BMI display
        }
    }
}

// Function to toggle password visibility
function togglePassword() {
    const passwordField = document.getElementById("password");
    const toggleIcon = document.getElementById("toggle-icon");

    // Toggle password visibility and icon
    if (passwordField.type === "password") {
        passwordField.type = "text";
        toggleIcon.src = "/static/images/show_password.png"; // Change to visible icon
    } else {
        passwordField.type = "password";
        toggleIcon.src = "/static/images/hide_password.png"; // Change to hidden icon
    }
}

// Function to calculate patient's BMI
function calculateBMI() {
    const heightInput = document.getElementById('height');
    const weightInput = document.getElementById('weight');
    const bmiValue = document.getElementById('bmi-value');
    const bmiCategory = document.getElementById('bmi-category');

    if (!heightInput || !weightInput || !bmiValue || !bmiCategory) {
        return;
    }

    const height = parseFloat(heightInput.value);
    const weight = parseFloat(weightInput.value);

    if (height > 0 && weight > 0) {
        const bmi = weight / (height * height);

        // Display BMI rounded to 1 decimal place
        bmiValue.textContent = bmi.toFixed(1);

        // Determine BMI category
        let category = '';
        let categoryClass = '';

        if (bmi < 18.5) {
            category = ' (Underweight)';
            categoryClass = 'underweight';
        } else if (bmi >= 18.5 && bmi < 25) {
            category = ' (Healthy)';
            categoryClass = 'healthy';
        } else if (bmi >= 25 && bmi < 30) {
            category = ' (Overweight)';
            categoryClass = 'overweight';
        } else {
            category = ' (Obese)';
            categoryClass = 'obese';
        }

        bmiCategory.textContent = category;
        bmiCategory.className = categoryClass;

    } else {
        bmiValue.textContent = '-';
        bmiCategory.textContent = '';
        bmiCategory.className = '';
    }
}

// Initialize the form when page loads
function initializeForm() {
    // Set up event listeners for BMI calculation (real-time updates)
    const heightInput = document.getElementById('height');
    const weightInput = document.getElementById('weight');
    
    if (heightInput && weightInput) {
        heightInput.addEventListener('input', calculateBMI);
        weightInput.addEventListener('input', calculateBMI);
        
        // Also calculate on page load in case values are pre-filled
        calculateBMI();
    }

    // Check if patient role is already selected and show fields
    const roleSelect = document.getElementById('role');
    if (roleSelect && roleSelect.value === 'patient') {
        handleRoleChange();
    }
}

// Run initialization when DOM is loaded
document.addEventListener('DOMContentLoaded', initializeForm);


// Function to expand the level description based on the lenght of the text
function enableAutoExpandTextarea(selector) {
    const textarea = document.querySelector(selector);

    if (textarea) {
        textarea.addEventListener("input", function () {
            this.style.height = "auto"; // Reset height
            this.style.height = this.scrollHeight + "px"; // Adjust height dynamically
        });
    }
}

// This function will attach the event listener to the button
function attachQRCodeListener() {
    var generateQRCodeBtn = document.getElementById("generateQRCodeBtn");

    // Check if the element exists before attaching the event listener
    if (generateQRCodeBtn) {
        generateQRCodeBtn.addEventListener("click", function() {
            var userId = this.getAttribute("data-user-id");
            qrcode_creator(userId);  // Call the function with the user ID
        });
    }
}

// This is the function that actually generates the QR code
function qrcode_creator(patient_id) {
    console.log("Generating QR Code for patient", patient_id);

    fetch('/homepage', {
        method: 'POST',  // Use POST to send data to the server
        headers: {
            'Content-Type': 'application/json'  // Ensure the Content-Type is application/json
        },
        body: JSON.stringify({ patient_id: patient_id })  // Send the patient ID
    })
    .then(response => response.json())
    .then(data => {
        if (data.qr_code) {
            // Display the QR code in the image element
            document.getElementById("qrCodeImage").src = `data:image/png;base64,${data.qr_code}`;
            // Show the QR code container and the toggle button
            document.getElementById("qrCodeContainer").style.display = "block";
            document.getElementById("toggleQRCodeBtn").style.display = "block";
        } else {
            console.error('Error:', data.error);  // Error handling
        }
    })
    .catch(error => console.error('Error:', error));
}

// Function that shows/hides QR Code
function toggleQRCodeVisibility() {
    var qrCodeContainer = document.getElementById("qrCodeContainer");
    var toggleButton = document.getElementById("toggleQRCodeBtn");

    if (qrCodeContainer.style.display === "none") {
        qrCodeContainer.style.display = "block"; // Show the QR code
        toggleButton.textContent = "Hide QR Code"; // Change the button text
    } else {
        qrCodeContainer.style.display = "none"; // Hide the QR code
        toggleButton.textContent = "Show QR Code"; // Change the button text
    }
}

// Function do show/collapse the level obstacle codes
function setupCollapsibles(containerSelector) {
    const container = document.querySelector(containerSelector);
    if (!container) return;

    const collapsibles = container.querySelectorAll(".collapsible");
    collapsibles.forEach((btn) => {
        btn.addEventListener("click", function () {
            this.classList.toggle("active");
            const content = this.nextElementSibling;
            if (content.style.maxHeight) {
                content.style.maxHeight = null;
            } else {
                content.style.maxHeight = content.scrollHeight + "px";
            }
        });
    });
}

// Function to check if the level_string is in the correct syntax
function setupLevelStringValidation(textareaSelector, feedbackSelector, submitButtonSelector) {
    const validCodes = new Set(["CL", "CM", "CR", "JL", "JM", "JR", "GND", "BS"]);
    const timeCodePattern = /^(LHLL|LHLM|LHLR|LHRL|LHRM|LHRR)(\d+)$/;

    const textarea = document.querySelector(textareaSelector);
    const feedback = document.querySelector(feedbackSelector);
    const submitButton = document.querySelector(submitButtonSelector);

    if (!textarea || !feedback || !submitButton) return;

    function validate() {
        const text = textarea.value.trim();
        let isValid = true;
        let messages = [];

        if (!text) {
            messages.push(" - Level string is required.");
            isValid = false;
        } else {
            const codes = text.split(",").map(code => code.trim());

            // 1. Check if it starts with "!"
            if (codes[0] !== "!") {
                messages.push(" - Must start with '!'.");
                isValid = false;
            }

            // 2. Check if there's at least one code after "!"
            if (codes.length < 2) {
                messages.push(" - At least one code after '!' is required.");
                isValid = false;
            }

            // 3. Validate each code
            for (let i = 1; i < codes.length; i++) {
                const code = codes[i];

                if (!code) {
                    messages.push(` - Missing code at position ${i + 1}.`);
                    isValid = false;
                } else if (!validCodes.has(code) && !timeCodePattern.test(code)) {
                    messages.push(` - Invalid code: '${code}'`);
                    isValid = false;
                }
            }
        }

        // Update feedback and submit button state
        if (!isValid) {
            feedback.innerHTML = messages.map(msg => `<div>${msg}</div>`).join("");
            feedback.style.color = "red";
            submitButton.disabled = true;
        } else {
            feedback.textContent = " - Level string is valid ✔️";
            feedback.style.color = "green";
            submitButton.disabled = false;
        }
    }

    // Initial state
    validate();
    textarea.addEventListener("input", validate);
}

