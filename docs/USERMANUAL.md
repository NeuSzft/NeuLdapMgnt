# Neu LDAP Management System - User Manual

*[**🠰** Back to the README](../README.md)*

### Table of Contents
1. [Introduction](#introduction)
1. [How to log in](#how-to-log-in)
2. [How to log out](#how-to-log-out)
3. [Managing classes](#managing-classes)
4. [Managing students](#managing-students)
5. [Managing administrators](#managing-administrators)
6. [Managing inactive users](#managing-inactive-users)
7. [Managing the default admin](#managing-the-default-admin)
8. [Inspecting logs](#inspecting-logs)
9. [How to import and export database](#how-to-import-and-export-database)

## Introduction

The **Neu LDAP Management System** is a versatile tool designed for managing user accounts within educational institutions. Key features include:

1. **User Authentication**: Simple login and logout processes.

2. **User and Role Management**: Administrators can create, inspect, edit, and deactivate accounts for students, teachers, and other administrators.

3. **Class Management**: Allows for the addition and removal of classes.

4. **Inactive User Management**: Manages inactive accounts with options to inspect and permanently delete them.

5. **Logs**: It supports reviewing the system logs to monitor activities and maintain security.

6. **Database Management**: Features for importing and exporting data and managing the default administrator to ensure system security.

This system is designed to streamline administrative processes and improve the management of educational user accounts.

## How to log in

In order to log in, fill in the **Username** and **Password** fields with your credentials and then press the **Log in** button.

## How to log out

In order to log out, navigate through the navbar and press the **Log out** button.

## Managing classes

### Adding a class

1. To add a class, navigate to **Database** 🠲 **Classes**. 

2. If there are no classes added yet, you will see a text indicating so.

3. Click the **Add** button located in the top right corner. 

4. In the modal window, input the name of the class and then press the **Add** button. 

5. The class will be displayed in a table. 

### Removing a class

1. To remove a class, click on the **Delete** button. 

2. Select the desired class to be deleted from the list. 

3. The class will be removed from the table.

## Managing students

### Creating a student

To create a student, navigate to **Students** 🠲 **Add Student** or **View Students**. It will offer the possibility to redirect you to the creation page if there are no students in the database yet.

On the student creation page, you will see a form that requires you to input various details about the student.

1. Fill in the OM field between 70000000000 and 79999999999.

2. Provide the student's first, middle (optional), and last names in the respective fields.

3. Select the appropriate class for the student. If no classes are available, you can add a class by referring to [Managing Classes](#managing-classes).

4. The email and password fields are optional. You can fill them in if desired.

After completing the form, click the **Submit** button. A summary of the student's details will appear. Click **Yes** to confirm and create the student, or **No** if you want to continue editing.

### Inspecting the students

1. Navigate to **Students** 🠲 **View Students**. This will display a table containing all active students.

2. The details shown in the table are limited: **OM**, **Name**, **Class**

To view further details scroll down to the next section.

### Inspecting a student

1. Navigate to **Students** 🠲 **View Students**. This will display a table containing all active students.

2. In the table, you will see inspect buttons with eye icons. Click on the button corresponding to the student you want to delete. This will redirect you to the student's details page.

On the student's details page, a similar form will be displayed when the student is created.

### Deactivating a student

To delete a student, follow these steps:

1. Navigate to **Students** 🠲 **View Students**. This will display a table containing all active students.

2. In the table, you will see inspect buttons with eye icons. Click on the button corresponding to the student you want to delete. This will redirect you to the student's details page.

3. On the student's details page, locate the edit button in the top right corner. Click on it and then select the **Deactivate** option. Confirm the action to deactivate the student.

4. The student will be removed from the table and moved to the **Database** 🠲 **Inactive Users** page.

### Deactivating students

To deactivate multiple students at once, follow these instructions:

1. Navigate to **Students** 🠲 **View Students**. This will display a table containing all active students.

2. In the first column of the table, you will see checkboxes next to each student's OM. Select the checkboxes corresponding to the students you want to delete. Alternatively, you can use the search bar to filter the students by OM, name, or class.

3. After selecting the desired students, click on the **Edit Selected** button located in the top right corner. A modal window will appear.

4. In the modal window, you will see a switch that can set the status of the selected students to inactive. Toggle the switch accordingly.

5. After setting the status, press the **Save**. Review the list of students whose status will be modified and confirm the action.

6. The selected students will be removed from the table and moved to **Database** 🠲 **Inactive Users** page.

## Managing teachers

### Creating a teacher

1. Navigate to **Teachers** 🠲 **Add Teacher** or **View Teachers**. It will offer the possibility to redirect you to the creation page if there are no teachers in the database yet.

2. the teacher creation page, you will see a form that requires you to input various details about the teacher.

3. Fill in the ID field.

4. Provide the teacher's first, middle (optional), and last names in the respective fields.

5. Select the class(optional) for the teacher. If no classes are available, you can add a class by referring to [Managing Classes](#managing-classes).

6. The email and password fields are optional. You can fill them in if desired.

After completing the form, click the **Submit** button. A summary of the teacher's details will appear. Click **Yes** to confirm and create the teacher, or **No** if you want to continue editing.

### Inspecting the teachers

1. Navigate to **Teachers** 🠲 **View Teachers**. This will display a table containing all active teachers.

2. The details shown in the table are limited: **ID**, Name, Class, Admin?

To view further details scroll down to the next section.

### Inspecting a teacher

1. Navigate to **Teachers** 🠲 **View Teachers**. This will display a table containing all active teachers.

2. In the table, you will see inspect buttons with eye icons. Click on the button corresponding to the teacher you want to delete. This will redirect you to the teacher's details page.

On the teacher's details page, a similar form will be displayed when the teacher is created.

### Deactivating a teacher

To delete a teacher, follow these steps:

1. Navigate to **Teachers** 🠲 **View Teachers**. This will display a table containing all active teachers.

2. In the table, you will see inspect buttons with eye icons. Click on the button corresponding to the teacher you want to delete. This will redirect you to the teacher's details page.

3. On the teacher's details page, locate the edit button in the top right corner. Click on it and then select the **Deactivate** option. Confirm the action to deactivate the teacher.

4. The teacher will be removed from the table and moved to the **Database** 🠲 **Inactive Users** page.

### Deactivating teachers

To deactivate multiple teachers at once, follow these instructions:

1. Navigate to **Teachers** 🠲 **View Teachers**. This will display a table containing all active teachers.

2. In the first column of the table, you will see checkboxes next to each teacher's ID. Select the checkboxes corresponding to the teachers you want to delete. Alternatively, you can use the search bar to filter the teachers by ID, name, or class.

3. After selecting the desired teachers, click on the **Edit Selected** button located in the top right corner. A modal window will appear.

4. In the modal window, you will see a switch that can set the status of the selected teachers to inactive. Toggle the switch accordingly.

5. After setting the status, press the **Save**. Review the list of teachers whose status will be modified and confirm the action.

6. The selected teachers will be removed from the table and moved to **Database** 🠲 **Inactive Users** page.

## Managing Administrators

### Creating administrators

1. Navigate to **Teachers** 🠲 **View Teachers**. This will display a table containing all active teachers.

2. In the first column of the table, you will see checkboxes next to each teacher's ID. Select the checkboxes corresponding to the teachers you want to make administrator. Alternatively, you can use the search bar to filter the teachers by ID, name, or class.

3. After selecting the desired teachers, click on the **Edit Selected** button located in the top right corner. A modal window will appear.

4. In the modal window, you will see a switch that can set the status of the selected teachers to administrator. Toggle the switch accordingly.

5. After setting the status, press the **Save**. Review the list of teachers whose status will be modified and confirm the action.

6. The selected teachers will be added to the administrators table and can be inspected on the **Database** 🠲 **Administrators** page.

### Inspecting administrators

1. Navigate to **Database** 🠲 **Administrators**. This will display a table containing all administrator IDs.

2. The details shown in the table are limited: **ID**

To view further details scroll down to the next section.

### Inspecting an administrator

1. Navigate to **Database** 🠲 **Administrators**. This will display a table containing all administrator IDs.

2. In the table, you will see inspect buttons with eye icons. Click on the button corresponding to the administrator's ID you want to inspect. This will redirect you to the administrator's (teacher's) details page.

3. On the administrator's details page, a similar form will be displayed when a teacher is created.

### Removing administrators

1. Navigate to **Database** 🠲 **Administrators**. This will display a table containing all administrator IDs.

2. In the first column of the table, you will see checkboxes next to each administrator's ID. Select the checkboxes corresponding to the administrators you want to delete.

3. After selecting the desired administrators, click on the **Remove selected** button located in the top right corner. A modal window will appear.

4. Review the list of administrators who will be removed and confirm the action.

6. The selected administrators will be removed from the table and the status of the teachers will be set to default (the **Admin?** column will not be checked). 

## Managing Inactive Users

### Inspecting inactive users

1. Navigate to **Database** 🠲 **Inactive Users**. This will display a table containing all inactive user IDs.

2. The details shown in the table are limited: **ID**

To view further details scroll down to the next section.

### Inspecting an inactive user

1. Navigate to **Database** 🠲 **Inactive Users**. This will display a table containing all inactive user IDs.

2. In the table, you will see inspect buttons with eye icons. Click on the button corresponding to the inactive user's ID you want to inspect. This will redirect you to the inactive user's (teacher/student) details page.

3. On the inactive user's details page, a similar form will be displayed when a teacher or a student is created.

### Deleting inactive users

**THIS ACTION CANNOT BE REVERTED!**

1. Navigate to **Database** 🠲 **Inactive Users**. This will display a table containing all inactive user IDs.

2. In the first column of the table, you will see checkboxes next to each inactive user's ID. Select the checkboxes corresponding to the inactive users you want to permanently delete.

3. After selecting the desired inactive users, click on the **Remove selected** button located in the top right corner. A modal window will appear.

4. Review the list of inactive users who will be **PERMANENTLY DELETED** and confirm the action.

6. The selected inactive users will be removed from the **database** and if the user had the administrator status, the ID will be removed from the administrators group as well.

## Inspecting logs

1. Navigate to **Database** 🠲 **Request Logs**. This will display a table containing all request logs.

2. The details shown in the table are:

    * ID
    * Timestamp
    * Log Level
    * Username
    * Full Name
    * Host
    * Method
    * Request Path
    * Response Code

To filter the logs by a specific time interval, first set the start and end dates using the provided fields. Once you have specified the desired time range, click the **Fetch** button.

This action will retrieve and display all logs recorded between the start and end dates you set.

## How to import and export database

Navigate to **Database** 🠲 **Import/Export**.

All details regarding these actions are provided on the page.

## Managing the default admin

Navigate to **Database** 🠲 **Default Admin**.

It is strongly advised to disable the default administrator by switching the input off after creating the initial administrators.

You have the option to change the default administrator's password. 

Fill in the form with the following fields:

* New password
* Confirm new password
