using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace NeuLdapMgnt.Models.ReadMeGenerator;

internal static class Program {
	private static string GetNameOfType(Type type) {
		StringBuilder sb = new();

		if (type.GenericTypeArguments.Any()) {
			sb.Append(type.Name[..type.Name.IndexOf('`')]);
			sb.Append("\\<");
			sb.Append(string.Join(", ", type.GenericTypeArguments.Select(x => x.Name)));
			sb.Append("\\>");
		}
		else if (type is { ContainsGenericParameters: true, IsGenericParameter: false, IsArray: false }) {
			sb.Append(type.Name[..type.Name.IndexOf('`')]);
			sb.Append("\\<T\\>");
		}
		else {
			sb.Append(type.Name);
		}

		return sb.ToString();
	}

	private static string GetAttributeName(Type type) {
		return type.Name[..type.Name.LastIndexOf("Attribute", StringComparison.InvariantCulture)];
	}

	private static string ModelToMarkdown(Type type) {
		StringBuilder sb = new();

		if (type.BaseType is { } baseType && baseType != typeof(object))
			sb.AppendLine($"## {GetNameOfType(type)} : {GetNameOfType(baseType)}");
		else
			sb.AppendLine($"## {GetNameOfType(type)}");
		sb.AppendLine();

		sb.AppendLine("### Attributes");
		foreach (string name in type.CustomAttributes.Select(x => GetAttributeName(x.AttributeType)))
			sb.AppendLine($"- {name}");
		sb.AppendLine();

		sb.AppendLine("### Properties");
		sb.AppendLine("|Type|Name|Attributes|");
		sb.AppendLine("|:---|:---|:---|");
		foreach (PropertyInfo info in type.GetProperties())
			sb.AppendLine($"|{GetNameOfType(info.PropertyType)}|{info.Name}|{string.Join(", ", info.CustomAttributes.Select(x => GetAttributeName(x.AttributeType)))}|");
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
			typeof(LdapAttributeAttribute),
			typeof(LdapObjectClassesAttribute),
			typeof(LdapDbDump),
			typeof(LogEntry),
			typeof(RequestResult),
			typeof(RequestResult<>),
			typeof(Person),
			typeof(Student),
			typeof(Teacher)
		);

		WriteMarkdownToFile(markdown);
	}
}
