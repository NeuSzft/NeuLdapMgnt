using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace NeuLdapMgnt.Models.ReadMeGenerator;

internal static class Program {
	private static string GetNameOfType(Type type, bool attrArg = false) {
		StringBuilder sb = new();

		if (type.GenericTypeArguments.Any()) {
			sb.Append(type.Name[..type.Name.IndexOf('`')]);
			sb.Append(@"\<");
			sb.Append(string.Join(", ", type.GenericTypeArguments.Select(x => x.Name)));
			sb.Append(@"\>");
		}
		else if (type is { ContainsGenericParameters: true, IsGenericParameter: false, IsArray: false }) {
			sb.Append(type.Name[..type.Name.IndexOf('`')]);
			sb.Append(attrArg ? "<>" : @"\<T\>");
		}
		else {
			sb.Append(type.Name);
		}

		return sb.ToString();
	}

	private static string GetAttributeName(CustomAttributeData data) {
		string type = data.AttributeType.Name;
		string name = type[..type.LastIndexOf("Attribute", StringComparison.InvariantCulture)];
		var    args = data.ConstructorArguments.Where(x => x.Value?.ToString() != x.Value?.GetType().ToString());
		return args.Any()
			? $"{name}(`{string.Join("`, `", args.Select(x => x.Value is Type value ? GetNameOfType(value, true) : x.Value))}`)"
			: name;
	}

	private static string ModelToMarkdown(Type type) {
		StringBuilder sb = new();

		if (type.BaseType is { } baseType && baseType != typeof(object))
			sb.AppendLine($"## {GetNameOfType(type)} : {GetNameOfType(baseType)}");
		else
			sb.AppendLine($"## {GetNameOfType(type)}");
		sb.AppendLine();

		sb.AppendLine("### Attributes");
		foreach (string name in type.CustomAttributes.Select(GetAttributeName))
			sb.AppendLine($"- {name}");
		sb.AppendLine();

		sb.AppendLine("### Properties");
		sb.AppendLine("|Type|Name|Attributes|");
		sb.AppendLine("|:---|:---|:---|");
		foreach (PropertyInfo info in type.GetProperties())
			sb.AppendLine($"|{GetNameOfType(info.PropertyType)}|{info.Name}|{string.Join(", ", info.CustomAttributes.Select(GetAttributeName))}|");
		sb.AppendLine();

		sb.AppendLine("### Methods");
		sb.AppendLine("|Return Type|Name|");
		sb.AppendLine("|:---|:---|");
		foreach (MethodInfo info in type.GetMethods().Where(x => !x.Name.Any(y => y is '_' or '$')))
			sb.AppendLine($"|{GetNameOfType(info.ReturnType)}|{info.Name}({string.Join(", ", info.GetParameters().Select(x => x.ParameterType.Name))})|");
		sb.AppendLine();

		return sb.ToString();
	}

	private static string ModelsToMarkdown(params Type[] types) {
		StringBuilder sb = new();

		sb.AppendLine("# Neu LDAP Management System - Models");
		sb.AppendLine();
		sb.AppendLine("*[**\ud83e\udc30** Back to the README](../README.md)*");
		sb.AppendLine();
		sb.AppendLine();

		foreach (Type type in types)
			sb.AppendLine(ModelToMarkdown(type));

		return sb.ToString();
	}

	private static void WriteMarkdownToFile(string md, [CallerFilePath] string srcPath = default!) {
		string path = Path.Join(Directory.GetParent(srcPath)!.Parent!.Parent!.FullName, "docs", "MODELS.md");
		File.WriteAllText(path, md);
	}

	private static void Main() {
		string markdown = ModelsToMarkdown(
			typeof(LdapDbDump),
			typeof(LogEntry),
			typeof(RequestResult),
			typeof(RequestResult<>),
			typeof(Person),
			typeof(Student),
			typeof(Employee)
		);

		WriteMarkdownToFile(markdown);
	}
}
