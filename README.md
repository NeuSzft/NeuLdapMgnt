# Neu LDAP Management System

### Table of Contents
1. [Dependencies](#dependencies)
2. [Running the Services](#running-the-services)
3. [Running the Demo](#running-the-demo)


## Dependencies
- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
- [GNU Make](https://www.gnu.org/software/make/)


## Running the Services
### Crate a .env file:
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
| DEFAULT_ADMIN_USERNAME | admin | The username of the default API admin |
| DEFAULT_ADMIN_PASSWORD | adminpass | The default password of the default API admin |
| WEB_PORT | 80 | The port to be used by the service |
| API_LOGS_DIR | `$(abspath $(dir .))/logs` | The directory to put the log files into. By default this is the **logs** directory within the makefile's directory. |


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
| 8080 | Nginx reverse proxy and web server |
| 8888 | phpLDAPadmin |

### Start the demo:
```
make demo
```

### Stop the demo:
```
make demo-stop
```

### Remove the demo containers:
```
make demo-down
```
