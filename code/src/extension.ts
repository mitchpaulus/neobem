import * as vscode from "vscode";
import {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind,
} from "vscode-languageclient/node";

let client: LanguageClient | undefined;

const LANGUAGE_ID = "neobem";
const OUTPUT_CHANNEL_NAME = "Neobem Language Server";

export async function activate(context: vscode.ExtensionContext): Promise<void> {
  context.subscriptions.push(
    vscode.commands.registerCommand("neobem.restartServer", async () => {
      await stopClient();
      await startClient(context);
    })
  );

  // Restart the server when the relevant settings change.
  context.subscriptions.push(
    vscode.workspace.onDidChangeConfiguration(async (event) => {
      if (
        event.affectsConfiguration("neobem.server.path") ||
        event.affectsConfiguration("neobem.server.enabled")
      ) {
        await stopClient();
        await startClient(context);
      }
    })
  );

  await startClient(context);
}

export async function deactivate(): Promise<void> {
  await stopClient();
}

async function startClient(context: vscode.ExtensionContext): Promise<void> {
  const config = vscode.workspace.getConfiguration("neobem");
  if (!config.get<boolean>("server.enabled", true)) {
    return;
  }

  const serverPath = config.get<string>("server.path", "nbem");

  const serverOptions: ServerOptions = {
    run: {
      command: serverPath,
      args: ["--lsp"],
      transport: TransportKind.stdio,
    },
    debug: {
      command: serverPath,
      args: ["--lsp"],
      transport: TransportKind.stdio,
    },
  };

  const clientOptions: LanguageClientOptions = {
    documentSelector: [
      { scheme: "file", language: LANGUAGE_ID },
      { scheme: "untitled", language: LANGUAGE_ID },
    ],
    outputChannelName: OUTPUT_CHANNEL_NAME,
  };

  client = new LanguageClient(
    LANGUAGE_ID,
    OUTPUT_CHANNEL_NAME,
    serverOptions,
    clientOptions
  );

  try {
    await client.start();
    context.subscriptions.push(client);
  } catch (error) {
    client = undefined;
    void vscode.window.showErrorMessage(
      `Could not start the Neobem language server using "${serverPath}". ` +
        `Make sure the Neobem executable (nbem) is installed and on your PATH, ` +
        `or set "neobem.server.path". Underlying error: ${String(error)}`
    );
  }
}

async function stopClient(): Promise<void> {
  if (!client) {
    return;
  }
  const current = client;
  client = undefined;
  try {
    await current.stop();
  } catch {
    // Ignore errors while stopping; the process may already be gone.
  }
}
