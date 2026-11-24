# CWE remediation notes

Changes made against the supplied CodeQL SARIF report:

- CWE-89: parameterized the AddHall name lookup and stored-procedure arguments; parameterized DemoVulnerableSearch.
- CWE-201: registration now accepts a user-chosen password and stores only its PasswordHasher hash; the password is no longer emailed.
- CWE-404 / CWE-459 / CWE-460: MailMessage instances are disposed with `using`, which also disposes attached resources.
- CWE-835: removed redundant nullable/conditional expressions and corrected hall capacity division/remainder.
- CWE-390 / CWE-391 / CWE-396: removed empty and catch-all exception handlers; database and SMTP failures are handled with specific exception types and logged.
- CWE-476: nullable DateOnly SQL formatting checks for null before accessing Value.
- CWE-563: removed unused assignments and locals.
- CWE-561: removed collections that were populated but never read.
- CWE-662: replaced `lock(this)` with a private lock object.

The uploaded SARIF report contained 33 results across 10 CWE-tagged CodeQL rules. The archive was edited based on those findings. A .NET SDK and CodeQL CLI were not available in this environment, so the project build and a fresh CodeQL scan could not be run here. Run `dotnet restore`, `dotnet build -c Release`, and your GitHub Actions CodeQL workflow after downloading.
