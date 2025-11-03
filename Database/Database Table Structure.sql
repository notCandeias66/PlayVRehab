BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Level" (
	"id"	INTEGER,
	"level_string"	TEXT,
	"description"	TEXT,
	PRIMARY KEY("id" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS "UserLevels" (
	"user_id"	INTEGER,
	"level_id"	INTEGER,
	"attempt"	INTEGER,
	"parameters"	TEXT DEFAULT 'No parameters specified',
	"score"	INTEGER DEFAULT 0,
	"time_taken"	INTEGER DEFAULT -1,
	"status"	TEXT DEFAULT 'Not Started',
	"game_results"	TEXT DEFAULT 'No results!',
	"observations"	TEXT DEFAULT 'Nothing to observe!',
	"progression"	TEXT,
	PRIMARY KEY("user_id","level_id","attempt"),
	FOREIGN KEY("level_id") REFERENCES "Level"("id"),
	FOREIGN KEY("user_id") REFERENCES "User"("id")
);
CREATE TABLE IF NOT EXISTS "User" (
	"id"	INTEGER UNIQUE,
	"name"	TEXT,
	"email"	TEXT UNIQUE,
	"weightX10"	INTEGER,
	"heightX100"	INTEGER,
	"bmiX10"	INTEGER,
	"bmi_class"	TEXT,
	"role"	TEXT,
	"password_hash"	TEXT NOT NULL,
	"physio_id"	INTEGER,
	"qrcode_number"	INTEGER DEFAULT 0,
	PRIMARY KEY("id"),
	FOREIGN KEY("physio_id") REFERENCES "User"("id")
);
