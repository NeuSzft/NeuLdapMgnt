# Neu LDAP Management System - Models

*[**🠰** Back to the README](../README.md)*


## LdapDbDump

### Attributes
- NullableContext(`1`)
- Nullable(`0`)
- RequiredMember

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|IEnumerable\<Student\>|Students|RequiredMember, JsonRequired, JsonPropertyName(`students`)|
|IEnumerable\<Employee\>|Employees|RequiredMember, JsonRequired, JsonPropertyName(`employees`)|
|Dictionary\<String, String\>|Values|RequiredMember, JsonRequired, JsonPropertyName(`values`)|

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
- NullableContext(`1`)
- Nullable(`0`)
- RequiredMember

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|BigInteger|Id||
|DateTime|Time|RequiredMember|
|String|LogLevel|RequiredMember|
|String|Username|Nullable(`2`)|
|String|FullName|Nullable(`2`)|
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
- NullableContext(`1`)
- Nullable(`0`)
- JsonSourceGenerationOptions
- JsonSerializable(`NeuLdapMgnt.Models.RequestResult`)

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|Int32|StatusCode|JsonRequired, JsonInclude, JsonPropertyName(`status_code`)|
|String[]|Errors|JsonRequired, JsonInclude, JsonPropertyName(`errors`)|
|String|NewToken|Nullable(`2`), JsonInclude, JsonPropertyName(`new_token`)|

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
- NullableContext(`1`)
- Nullable(`0`)

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|T[]|Values|JsonRequired, JsonInclude, JsonPropertyName(`values`)|
|T|Value|Nullable(`2`), JsonIgnore|
|Int32|StatusCode|JsonRequired, JsonInclude, JsonPropertyName(`status_code`)|
|String[]|Errors|JsonRequired, JsonInclude, JsonPropertyName(`errors`)|
|String|NewToken|Nullable(`2`), JsonInclude, JsonPropertyName(`new_token`)|

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


## Person

### Attributes
- NullableContext(`1`)
- Nullable(`0`)
- LdapObjectClasses

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|Int32|Uid|Required, JsonRequired, JsonPropertyName(`uid`), LdapAttribute(`uidNumber`, `False`)|
|Int32|Gid|Required, JsonRequired, JsonPropertyName(`gid`), LdapAttribute(`gidNumber`, `False`)|
|String|Username|Required, JsonRequired, JsonPropertyName(`username`), LdapAttribute(`cn`, `False`)|
|String|GivenName|Required, GivenName, JsonRequired, JsonPropertyName(`first_name`), LdapAttribute(`givenName`, `False`)|
|String|Surname|Required, Surname, JsonRequired, JsonPropertyName(`last_name`), LdapAttribute(`sn`, `False`)|
|String|MiddleName|Nullable(`2`), MiddleName, JsonInclude, JsonPropertyName(`middle_name`), LdapAttribute(`title`, `False`)|
|String|Email|Nullable(`2`), Email, JsonRequired, JsonPropertyName(`email`), LdapAttribute(`mail`, `False`)|
|String|HomeDirectory|Required, Directory, JsonRequired, JsonPropertyName(`home_directory`), LdapAttribute(`homeDirectory`, `False`)|
|String|Password|Nullable(`2`), Password, PasswordPropertyText(`True`), JsonInclude, JsonPropertyName(`password`), LdapAttribute(`userPassword`, `True`)|
|String|FullName|JsonPropertyName(`full_name`), LdapAttribute(`displayName`, `False`)|
|Boolean|IsInactive|JsonInclude, JsonPropertyName(`is_inactive`), LdapFlag(`inactive`)|

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
- NullableContext(`1`)
- Nullable(`0`)

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|Int64|Id|Required, IdStudent(`70000000000`, `79999999999`), JsonPropertyName(`id`), LdapAttribute(`uid`, `False`)|
|Int32|Uid|Required, UserId(`6000`, `9999`), JsonRequired, JsonPropertyName(`uid`), LdapAttribute(`uidNumber`, `False`)|
|Int32|Gid|Required, GroupId(`6000`, `9999`), JsonRequired, JsonPropertyName(`gid`), LdapAttribute(`gidNumber`, `False`)|
|String|Class|Required, JsonRequired, JsonPropertyName(`class`), LdapAttribute(`roomNumber`, `False`)|
|String|Username|Required, JsonRequired, JsonPropertyName(`username`), LdapAttribute(`cn`, `False`)|
|String|GivenName|Required, GivenName, JsonRequired, JsonPropertyName(`first_name`), LdapAttribute(`givenName`, `False`)|
|String|Surname|Required, Surname, JsonRequired, JsonPropertyName(`last_name`), LdapAttribute(`sn`, `False`)|
|String|MiddleName|Nullable(`2`), MiddleName, JsonInclude, JsonPropertyName(`middle_name`), LdapAttribute(`title`, `False`)|
|String|Email|Nullable(`2`), Email, JsonRequired, JsonPropertyName(`email`), LdapAttribute(`mail`, `False`)|
|String|HomeDirectory|Required, Directory, JsonRequired, JsonPropertyName(`home_directory`), LdapAttribute(`homeDirectory`, `False`)|
|String|Password|Nullable(`2`), Password, PasswordPropertyText(`True`), JsonInclude, JsonPropertyName(`password`), LdapAttribute(`userPassword`, `True`)|
|String|FullName|JsonPropertyName(`full_name`), LdapAttribute(`displayName`, `False`)|
|Boolean|IsInactive|JsonInclude, JsonPropertyName(`is_inactive`), LdapFlag(`inactive`)|

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
- NullableContext(`1`)
- Nullable(`0`)

### Properties
|Type|Name|Attributes|
|:---|:---|:---|
|String|Id|Required, IdEmployee, JsonPropertyName(`id`), LdapAttribute(`uid`, `False`)|
|Int32|Uid|Required, UserId(`4000`, `5999`), JsonRequired, JsonPropertyName(`uid`), LdapAttribute(`uidNumber`, `False`)|
|Int32|Gid|Required, UserId(`4000`, `5999`), JsonRequired, JsonPropertyName(`gid`), LdapAttribute(`gidNumber`, `False`)|
|String|Class|JsonRequired, JsonPropertyName(`class`), LdapAttribute(`roomNumber`, `False`)|
|Boolean|IsAdmin|JsonInclude, JsonPropertyName(`is_admin`), LdapFlag(`admin`)|
|Boolean|IsTeacher|JsonInclude, JsonPropertyName(`is_teacher`), LdapFlag(`teacher`)|
|String|Username|Required, JsonRequired, JsonPropertyName(`username`), LdapAttribute(`cn`, `False`)|
|String|GivenName|Required, GivenName, JsonRequired, JsonPropertyName(`first_name`), LdapAttribute(`givenName`, `False`)|
|String|Surname|Required, Surname, JsonRequired, JsonPropertyName(`last_name`), LdapAttribute(`sn`, `False`)|
|String|MiddleName|Nullable(`2`), MiddleName, JsonInclude, JsonPropertyName(`middle_name`), LdapAttribute(`title`, `False`)|
|String|Email|Nullable(`2`), Email, JsonRequired, JsonPropertyName(`email`), LdapAttribute(`mail`, `False`)|
|String|HomeDirectory|Required, Directory, JsonRequired, JsonPropertyName(`home_directory`), LdapAttribute(`homeDirectory`, `False`)|
|String|Password|Nullable(`2`), Password, PasswordPropertyText(`True`), JsonInclude, JsonPropertyName(`password`), LdapAttribute(`userPassword`, `True`)|
|String|FullName|JsonPropertyName(`full_name`), LdapAttribute(`displayName`, `False`)|
|Boolean|IsInactive|JsonInclude, JsonPropertyName(`is_inactive`), LdapFlag(`inactive`)|

### Methods
|Return Type|Name|
|:---|:---|
|Boolean|Equals(Employee)|
|Boolean|Equals(Object)|
|Int32|GetHashCode()|
|String|GetUsername()|
|Type|GetType()|
|String|ToString()|


