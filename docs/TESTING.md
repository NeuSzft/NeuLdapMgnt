# Neu LDAP Management System - Testing

*[**🠰** Back to the README](../README.md)*

### Table of Contents
1. [Running All Tests](#running-all-tests)
2. [Running just the API Tests](#running-just-the-api-tests)
3. [Running just the Selenium Tests](#running-just-the-selenium-tests)
4. [Running just the Model Validation Tests](#running-just-the-model-validation-tests)


## Running all Tests

#### Command to run the tests:
```
make tests
```


## Running just the API Tests
These tests are meant to check the different services created for the API to interact with the LDAP server and the authentication of its endpoints.

#### Command to run the tests:
```
make api-tests
```

> The results of the api tests can be found [here](./test-results/api-test-results.md).


## Running just the Selenium Tests

#### Command to run the tests:
```
make web-tests
```

> The results of the selenium tests can be found within the [Webapp.Test](../NeuLdapMgnt/Webapp.Tests/) directory.


## Running just the Model Validation Tests

#### Command to run the tests:
```
make model-tests
```
> The results of unit tests can be found within the [Models.Test](../NeuLdapMgnt/Models.Tests/) directory.
