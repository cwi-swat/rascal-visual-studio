package org.rascalmpl.visualstudio.analysis.server;

import java.util.Collections;
import java.util.List;

import org.eclipse.lsp4j.Diagnostic;
import org.eclipse.lsp4j.TextDocumentIdentifier;

public class BasicAnalysisServer implements IAnalysisServer {

    @Override
    public List<Diagnostic> analyze(TextDocumentIdentifier document) {
        return Collections.emptyList();
    }
    
}
