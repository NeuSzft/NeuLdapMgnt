# Neu LDAP Management System

*[**🠰** Back to README](../README.md)*

### Table of Contents
1. [Running All Tests](#running-all-tests)
2. [Running just the API Tests](#running-just-the-api-tests)
3. [Running just the Selenium Tests](#running-just-the-selenium-tests)
4. [Running just the Model Validation Tests](#running-just-the-model-validation-tests)


## Running All Tests

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

> The results of the api tests can be found within the [/docs/test-results](./test-results/) directory.


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