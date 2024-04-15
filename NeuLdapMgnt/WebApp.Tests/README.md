
# Test Run
### Run Summary

<p>
<strong>Overall Result:</strong> ❌ Fail <br />
<strong>Pass Rate:</strong> 88.89% <br />
<strong>Run Duration:</strong> 35s 442ms <br />
<strong>Date:</strong> 2024-04-15 20:52:54 - 2024-04-15 20:53:29 <br />
<strong>Framework:</strong> .NETCoreApp,Version=v7.0 <br />
<strong>Total Tests:</strong> 9 <br />
</p>

<table>
<thead>
<tr>
<th>✔️ Passed</th>
<th>❌ Failed</th>
<th>⚠️ Skipped</th>
</tr>
</thead>
<tbody>
<tr>
<td>8</td>
<td>1</td>
<td>0</td>
</tr>
<tr>
<td>88.89%</td>
<td>11.11%</td>
<td>0%</td>
</tr>
</tbody>
</table>

### Result Sets
#### NeuLdapMgnt.WebApp.Tests.dll - 88.89%
<details>
<summary>Full Results</summary>
<table>
<thead>
<tr>
<th>Result</th>
<th>Test</th>
<th>Duration</th>
</tr>
</thead>
<tr>
<td> ✔️ Passed </td>
<td>DefaultRedirectionToLoginPage</td>
<td>1s 359ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>RedirectionIsWorkingWhenUnauthorized</td>
<td>6s 794ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>SuccessfulLoginRedirectsToHomePage</td>
<td>2s 99ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>NavbarLinksAreWorking</td>
<td>5s 67ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>NoStudentsArePresent</td>
<td>2s 564ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>NoStudentsArePresentAndAddStudentsButtonIsPresent</td>
<td>2s 509ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>AfterPressingAddStudentsButtonRedirectsToAddStudent</td>
<td>2s 820ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>AddStudentsEditFormHasLoadedDefaultValues</td>
<td>3s 32ms</td>
</tr>
<tr>
<td> ❌ Failed </td>
<td>AddStudentsEditFormIsValidatingCorrectly<blockquote><details>
<summary>Error</summary>
<strong>Message:</strong>
<pre><code>Assert.AreEqual failed. Expected:<The field Id must be between 70000000000 and 79999999999.>. Actual:<OM must be between 70000000000 and 79999999999.>. </code></pre>
<strong>Stack Trace:</strong>
<pre><code>   at NeuLdapMgnt.WebApp.Tests.SeleniumTests.AddStudentsEditFormIsValidatingCorrectly() in /app/WebApp.Tests/SeleniumTests.cs:line 236
   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
   at System.Reflection.MethodInvoker.Invoke(Object obj, IntPtr* args, BindingFlags invokeAttr)
</code></pre>
</details></blockquote>
</td>
<td>2s 925ms</td>
</tr>
</tbody>
</table>
</details>

### Run Messages
<details>
<summary>Informational</summary>
<pre><code>
</code></pre>
</details>

<details>
<summary>Warning</summary>
<pre><code>
</code></pre>
</details>

<details>
<summary>Error</summary>
<pre><code>
</code></pre>
</details>



----

[Created using Liquid Test Reports](https://github.com/kurtmkurtm/LiquidTestReports)