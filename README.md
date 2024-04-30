# Neu LDAP Management System

### Table of Contents
1. [Project Summary](#project-summary)
2. [Dependencies](#dependencies)
3. [Databases](#databases)
    - [PostgreSQL](#postgresql)
    - [OpenLDAP](#openldap)
4. [Running the Project](#running-the-project)
    1. [Crate a *.env* file](#crate-a-env-file)
    2. [Environment variables](#environment-variables)
    3. [Managing the services](#start-the-services)
5. [Running the Demo](#running-the-demo)
6. [Testing](#testing)


## Project Summary
**TODO!**


## Dependencies
- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
- [GNU Make](https://www.gnu.org/software/make/)

## Databases

### PostgreSQL
The Postgres database is responsible for storing the logs the API creates when it receives incoming requests and the users who have sent the requests.

*[Read more about the Postgres database](./docs/POSTGRES.md)*

### OpenLDAP
The LDAP database is responsible for storing the teachers and students of the school.
This includes their personal information and credentials. The LDAP database is also used for authenticating teachers who are admins, thus have access to the API.

*[Read more about the LDAP database](./docs/POSTGRES.md)*


## Running the Project

### Crate a *.env* file:
Run this command:
```
make env
```

This will generate a .env that will contain the necessary environment variables. You can edit these if needed.
> If the defaults are okay, then this can be skipped because it is a prerequisite of the `start` target.

### Environment variables:
| Name | Default | Description |
|:---|:---|:---|
| LDAP_ORGANIZATION | Neu | The name of the organization |
| LDAP_DOMAIN | neu.local | The name of the LDAP domain |
| LDAP_ADMIN_PASSWORD | ldappass | The password of the **admin** LDAP user |
| POSTGRES_USER_PASSWORD | postgres | The password of the **postgres** PostgreSQL user |
| DEFAULT_ADMIN_USERNAME | admin | The username of the default API admin |
| DEFAULT_ADMIN_PASSWORD | adminpass | The default password of the default API admin |
| LOG_TO_DB | true | If `true` the API will log the incoming requests to the Postgres database |
| PORT | 80 | The local port to be used by Nginx |

### Start the services:
```
make
```
or
```
make start
```

### Stop the services:
```
make stop
```

### Remove the containers:
```
make down
```


## Running the Demo
The demo enables some debugging and testing features and exposes some additional ports for management.

| Port | Service |
|:---|:---|
| 389 | OpenLDAP |
| 5000 | API |
| 5432 | PostgreSQL |
| 8080 | Nginx reverse proxy and web server |
| 8888 | phpLDAPadmin |

#### Run this command to start the demo:
```
make demo
```

#### Run this command to stop the demo:
```
make demo-stop
```

#### Run this command to remove the demo containers:
```
make demo-down
```


## Testing
There are three major parts of the that can be tested separately, 
- The API endpoints and services.
- The model validation
- The functionality of the webapp

> All of these groups of tests run isolated in docker containers.

*[Read more about testing](./docs/TESTING.md.md)*
