package org.rascalmpl.visualstudio.analysis.server;

import java.util.List;

import org.eclipse.lsp4j.Diagnostic;
import org.eclipse.lsp4j.TextDocumentIdentifier;


public interface IAnalysisServer {
   
    List<Diagnostic> analyze(TextDocumentIdentifier document);

}
