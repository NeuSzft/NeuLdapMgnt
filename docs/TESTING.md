# Neu LDAP Management System - Testing

*[**🠰** Back to the README](../README.md)*

### Table of Contents
1. [Running all Tests](#running-all-tests)
2. [Running just the Model Validation Tests](#running-just-the-model-validation-tests)
3. [Running just the API Tests](#running-just-the-api-tests)
4. [Running just the Selenium Tests](#running-just-the-selenium-tests)


## Running all Tests

#### Command to run the tests:
```
make tests
```


## Running just the Model Validation Tests
These tests are meant to check the validation of the models' fields.

#### Command to run the tests:
```
make model-tests
```


## Running just the API Tests
These tests are meant to check the different services created for the API to interact with the LDAP server and the authentication of its endpoints.

#### Command to run the tests:
```
make api-tests
```


## Running just the Selenium Tests

#### Command to run the tests:
```
make web-tests
```
