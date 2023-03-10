# rascal-visual-studio-analysis-server

:::warning
This project is under development; not even alpha status
:::

This project offers a JSON-RPC server for handling source code analysis requests which
are generated by the dual rascal-visual-studio-analysis-client project. That client runs
in Microsoft Visual Studio (**not Visual Studio Code**).

:::info
This is not a language server protocol server for Microsoft Visual Studio.
For Rascal LSP server (genenerators) see [rascal-language-servers](http://github.com/usethesource/rascal-language-servers).
:::

This project features:

* Remote method invocation of analyses via JSON-RPC
* Dynamic loading, configuration and running of Rascal analysis scripts
* Integration of the [clair](http://github.com/usethesource/clair) C/C++ analysis front-end for Rascal
* Execution of bespoke analysis scripts and reporting of issues in the user-interface of the programmer of Visual Studio.

