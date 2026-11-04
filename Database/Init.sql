CREATE DOMAIN email_t AS VARCHAR(100) CHECK(VALUE ~ '^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$');

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE admin(
    id UUID NOT NULL DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    email email_t NOT NULL,
	date_of_birth DATE NOT NULL,
    password_hash TEXT NOT NULL,

    CONSTRAINT pk_admin PRIMARY KEY(id),
	CONSTRAINT uq_admin_email UNIQUE(email)
);

CREATE TABLE department(
    id UUID NOT NULL DEFAULT gen_random_uuid(),
    major INT NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,

    admin_id UUID,

    CONSTRAINT pk_department PRIMARY KEY(id),
    CONSTRAINT chk_department_hours CHECK(start_time<end_time),

    CONSTRAINT fk_department_admin FOREIGN KEY (admin_id) REFERENCES admin(id)
);

CREATE TABLE employee(
    id UUID NOT NULL DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    email email_t NOT NULL,
	date_of_birth DATE NOT NULL,
    password_hash TEXT NOT NULL,
	account_status VARCHAR(30) NOT NULL,
    role VARCHAR(30) NOT NULL,

    CONSTRAINT pk_employee PRIMARY KEY(id),
	CONSTRAINT uq_employee_email UNIQUE(email),
	CONSTRAINT chk_employee_account_status CHECK(account_status IN('active', 'inactive')),
    CONSTRAINT chk_employee_role CHECK(role IN('default', 'cleaner'))
);

CREATE TABLE desk(
    id UUID NOT NULL DEFAULT gen_random_uuid(),
    minor INT NOT NULL,
    preferred_height FLOAT NOT NULL,
    company_height FLOAT NOT NULL,

    employee_id UUID,
    department_id UUID,

    CONSTRAINT pk_desk PRIMARY KEY (id),

    CONSTRAINT fk_desk_employee FOREIGN KEY (employee_id) REFERENCES employee(id),
    CONSTRAINT fk_desk_department FOREIGN KEY (department_id) REFERENCES department(id)
);

CREATE TABLE works_in(
    employee_id UUID,
    department_id UUID,

    start_time TIME NOT NULL,
    end_time TIME NOT NULL,

    CONSTRAINT pk_works_in PRIMARY KEY(employee_id,department_id),
    CONSTRAINT chk_works_in_cleaning_hours CHECK(start_time<end_time),

    CONSTRAINT fk_works_in_employee FOREIGN KEY(employee_id) REFERENCES employee(id),
    CONSTRAINT fk_works_in_department FOREIGN KEY(department_id) REFERENCES department(id)
);