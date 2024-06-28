# Neu LDAP Management System - PostgreSQL

*[**🠰** Back to the README](../README.md)*

The two tables used by the database are the **entries** and **users** tables.
The **entries** table stores the logs the API creates and the **users** table stores the users who have made the requests that resulted in the logs.

The tables are generate by the API server when it starts if logging to the database is enabled and the tables do not already exists.

![Postgres Tables](./postgres-tables.svg)
