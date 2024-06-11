# Neu LDAP Management System - Models

*[**🠰** Back to the README](../README.md)*


## LdapDbDump

### Attributes
- NullableContext
- Nullable
- RequiredMember

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|IEnumerable\<Student\>|Students|RequiredMember, JsonRequired, JsonPropertyName|
|IEnumerable\<Employee\>|Employees|RequiredMember, JsonRequired, JsonPropertyName|
|Dictionary\<String, String\>|Values|RequiredMember, JsonRequired, JsonPropertyName|

### Methods
|Return Type|Name|
|:---|:---|
|String|ToString()|
|Int32|GetHashCode()|
|Boolean|Equals(Object)|
|Boolean|Equals(LdapDbDump)|
|Type|GetType()|


## LogEntry

### Attributes
- NullableContext
- Nullable
- RequiredMember

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|BigInteger|Id||
|DateTime|Time|RequiredMember|
|String|LogLevel|RequiredMember|
|String|Username|Nullable|
|String|FullName|Nullable|
|String|Host|RequiredMember|
|String|Method|RequiredMember|
|String|RequestPath|RequiredMember|
|Int32|StatusCode|RequiredMember|

### Methods
|Return Type|Name|
|:---|:---|
|String|ToString()|
|String|ToTsv(LogEntry)|
|LogEntry|FromTsv(String)|
|Type|GetType()|
|Boolean|Equals(Object)|
|Int32|GetHashCode()|


## RequestResult

### Attributes
- NullableContext
- Nullable
- JsonSourceGenerationOptions
- JsonSerializable

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|Int32|StatusCode|JsonRequired, JsonInclude, JsonPropertyName|
|String[]|Errors|JsonRequired, JsonInclude, JsonPropertyName|
|String|NewToken|Nullable, JsonInclude, JsonPropertyName|

### Methods
|Return Type|Name|
|:---|:---|
|String|GetError()|
|Boolean|IsSuccess()|
|Boolean|IsFailure()|
|Type|GetType()|
|String|ToString()|
|Boolean|Equals(Object)|
|Int32|GetHashCode()|


## RequestResult\<T\> : RequestResult

### Attributes
- NullableContext
- Nullable

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|T[]|Values|JsonRequired, JsonInclude, JsonPropertyName|
|T|Value|Nullable, JsonIgnore|
|Int32|StatusCode|JsonRequired, JsonInclude, JsonPropertyName|
|String[]|Errors|JsonRequired, JsonInclude, JsonPropertyName|
|String|NewToken|Nullable, JsonInclude, JsonPropertyName|

### Methods
|Return Type|Name|
|:---|:---|
|T|GetValue()|
|String|GetError()|
|Boolean|IsSuccess()|
|Boolean|IsFailure()|
|Type|GetType()|
|String|ToString()|
|Boolean|Equals(Object)|
|Int32|GetHashCode()|


## Person

### Attributes
- NullableContext
- Nullable
- LdapObjectClasses

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|Int32|Uid|Required, JsonRequired, JsonPropertyName, LdapAttribute|
|Int32|Gid|Required, JsonRequired, JsonPropertyName, LdapAttribute|
|String|Username|Required, JsonRequired, JsonPropertyName, LdapAttribute|
|String|FirstName|Required, FirstName, JsonRequired, JsonPropertyName, LdapAttribute|
|String|LastName|Required, LastName, JsonRequired, JsonPropertyName, LdapAttribute|
|String|MiddleName|Nullable, MiddleName, JsonInclude, JsonPropertyName, LdapAttribute|
|String|Email|Nullable, Email, JsonRequired, JsonPropertyName, LdapAttribute|
|String|HomeDirectory|Required, Directory, JsonRequired, JsonPropertyName, LdapAttribute|
|String|Password|Nullable, Password, PasswordPropertyText, JsonInclude, JsonPropertyName, LdapAttribute|
|String|FullName|JsonPropertyName, LdapAttribute|
|Boolean|IsInactive|JsonInclude, JsonPropertyName, LdapFlag|

### Methods
|Return Type|Name|
|:---|:---|
|String|GetUsername()|
|Type|GetType()|
|String|ToString()|
|Boolean|Equals(Object)|
|Int32|GetHashCode()|


## Student : Person

### Attributes
- NullableContext
- Nullable

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|Int64|Id|Required, IdStudent, JsonPropertyName, LdapAttribute|
|Int32|Uid|Required, UserId, JsonRequired, JsonPropertyName, LdapAttribute|
|Int32|Gid|Required, GroupId, JsonRequired, JsonPropertyName, LdapAttribute|
|String|Class|Required, JsonRequired, JsonPropertyName, LdapAttribute|
|String|Username|Required, JsonRequired, JsonPropertyName, LdapAttribute|
|String|FirstName|Required, FirstName, JsonRequired, JsonPropertyName, LdapAttribute|
|String|LastName|Required, LastName, JsonRequired, JsonPropertyName, LdapAttribute|
|String|MiddleName|Nullable, MiddleName, JsonInclude, JsonPropertyName, LdapAttribute|
|String|Email|Nullable, Email, JsonRequired, JsonPropertyName, LdapAttribute|
|String|HomeDirectory|Required, Directory, JsonRequired, JsonPropertyName, LdapAttribute|
|String|Password|Nullable, Password, PasswordPropertyText, JsonInclude, JsonPropertyName, LdapAttribute|
|String|FullName|JsonPropertyName, LdapAttribute|
|Boolean|IsInactive|JsonInclude, JsonPropertyName, LdapFlag|

### Methods
|Return Type|Name|
|:---|:---|
|Boolean|Equals(Student)|
|Boolean|Equals(Object)|
|Int32|GetHashCode()|
|String|GetUsername()|
|Type|GetType()|
|String|ToString()|


## Employee : Person

### Attributes
- NullableContext
- Nullable

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|String|Id|Required, IdEmployee, JsonPropertyName, LdapAttribute|
|Int32|Uid|Required, UserId, JsonRequired, JsonPropertyName, LdapAttribute|
|Int32|Gid|Required, UserId, JsonRequired, JsonPropertyName, LdapAttribute|
|String|Class|JsonRequired, JsonPropertyName, LdapAttribute|
|Boolean|IsAdmin|JsonInclude, JsonPropertyName, LdapFlag|
|Boolean|IsTeacher|JsonInclude, JsonPropertyName, LdapFlag|
|String|Username|Required, JsonRequired, JsonPropertyName, LdapAttribute|
|String|FirstName|Required, FirstName, JsonRequired, JsonPropertyName, LdapAttribute|
|String|LastName|Required, LastName, JsonRequired, JsonPropertyName, LdapAttribute|
|String|MiddleName|Nullable, MiddleName, JsonInclude, JsonPropertyName, LdapAttribute|
|String|Email|Nullable, Email, JsonRequired, JsonPropertyName, LdapAttribute|
|String|HomeDirectory|Required, Directory, JsonRequired, JsonPropertyName, LdapAttribute|
|String|Password|Nullable, Password, PasswordPropertyText, JsonInclude, JsonPropertyName, LdapAttribute|
|String|FullName|JsonPropertyName, LdapAttribute|
|Boolean|IsInactive|JsonInclude, JsonPropertyName, LdapFlag|

### Methods
|Return Type|Name|
|:---|:---|
|Boolean|Equals(Employee)|
|Boolean|Equals(Object)|
|Int32|GetHashCode()|
|String|GetUsername()|
|Type|GetType()|
|String|ToString()|


