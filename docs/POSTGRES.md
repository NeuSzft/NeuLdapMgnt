# Neu LDAP Management System - PostgreSQL

*[**🠰** Back to the README](../README.md)*

The two tables used by the database are the **entries** and **users** tables.
The **entries** table stores the logs the API creates and the **users** table stores the users who have made the requests that resulted in the logs.

The tables are generate by the API server when it starts if logging to the database is enabled and the tables do not already exists.

```
┌──────────────────────────────────┐
│ entries                          │
├───────────────────┬──────────────┤
│ [PK] bigint       │ id           │
│      timestamp    │ time         │
│      varchar(11)  │ log_level    │
│ [FK] varchar(255) │ username     │─N┐
│      varchar(255) │ host         │  │
│      varchar(7)   │ method       │  │
│      varchar(255) │ request_path │  │
│      int          │ statusCode   │  │
└───────────────────┴──────────────┘  │
                                      │
┌──────────────────────────────────┐  │
│ users                            │  │
├───────────────────┬──────────────┤  │
│ [PK] varchar(255) │ username     │─1┘
│      varchar(11)  │ full_name    │
└───────────────────┴──────────────┘
```
