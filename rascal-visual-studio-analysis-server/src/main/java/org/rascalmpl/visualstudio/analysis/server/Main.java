package org.rascalmpl.visualstudio.analysis.server;

import org.eclipse.lsp4j.jsonrpc.Launcher;

public class Main {

    public static void main(String[] args) {
        IAnalysisServer server = new BasicAnalysisServer();

        Launcher<IAnalysisServer> launch = new Launcher.Builder<IAnalysisServer>()
            .setInput(System.in)
            .setOutput(System.out)
            .setLocalService(server)
            .setRemoteInterface(IAnalysisServer.class)
            .create();
        ;

        launch.startListening();
    }
}
