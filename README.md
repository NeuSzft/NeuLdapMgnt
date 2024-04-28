# Neu LDAP Management System

### Table of Contents
1. [Dependencies](#dependencies)
2. [Running the Project](#running-the-project)
    1. [Crate a *.env* file](#crate-a-env-file)
    2. [Environment variables](#environment-variables)
    3. [Manage the services](#start-the-services)
3. [Running the Demo](#running-the-demo)
4. [Running the tests](#running-the-tests)
5. [**TODO:** PostgreSQL database](#table-of-contents)
6. [**TODO:** OpenLDAP database](#table-of-contents)

## Dependencies
- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
- [GNU Make](https://www.gnu.org/software/make/)


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


## Running the tests

*[**🠲** Continue to TESTING](./docs/TESTING.md)*
