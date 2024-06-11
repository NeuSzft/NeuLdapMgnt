# Neu LDAP Management System - OpenLDAP

*[**🠰** Back to the README](../README.md)*

### Table of Contents
1. [Overview](#overview)
2. [Default Admin](#default-admin)
3. [Entities](#entities)
4. [Key-value pairs](#key-value-pairs)


## Overview
The LDAP database is an essential part of this project.
It is responsible for storing the employees, students, classes and which employees have administrator privileges.

The API handles these split into [entities](#entities) and [key-value pairs](#key-value-pairs).

The employees and students are entities.\
If an employee or student is no longer needed they can be made inactive using the `inactive` flag, which prevents employees if they are admins from logging in.\
An employee is an admin if they have the `admin` flag.
The classes that the students can be a part of or the employees (teachers) can be in charge of are stored as a key-value pair under the name `classes`. 
The password of the [default admin](#default-admin) and wether or not it is enabled is also stored as two key value pairs.


## Default Admin
The default admin acts as an admin user, but does not actually exist as an entity.\
It instead is defined by two [environment variables](../README.md#environment-variables).

The `DEFAULT_ADMIN_USERNAME` env specifies the username the default admin should use to authenticate itself and is not stored within the database and is read each time the API starts.

The `DEFAULT_ADMIN_PASSWORD` env on the other hand is hashed using bcrypt and put into the database's `default-admin-password` key-value pair when the default admin is created by the API.

The other key-value pair of the default admin is the `default-admin-enabled` which is set to `true` by default. It specifies wether the default admin is enabled, meaning it is able to login or not.

> If the default admin's key-value pairs are deleted they are automatically recreated by the API.


## Entities
The entities always have an associated [model](../NeuLdapMgnt/Models/) which defines how it should be handled by the API.

The class of the models can use the [LdapObjectClasses](../NeuLdapMgnt/Models/LdapAttributes.cs) attribute to specify the object classes they should have within the LDAP database.

The properties of these classes can use the [LdapAttribute](../NeuLdapMgnt/Models/LdapAttributes.cs) attribute to specify which LDAP attribute they should bind to and if they count as hidden or not.\
If an attribute is marked as hidden, then it is not set or returned from the database by default. An example for this is the `userPassword` which does not need to be updated or returned in most cases.


## Key-value pairs
Key-value pairs are meant to store data that does not have a complex type like an entity. They are identified by a string key and can store a string value.\
For example, the `default-admin-enabled` key can store a boolean in string from.
