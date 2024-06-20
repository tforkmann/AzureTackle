import { createElement } from "react";
import React from "react";
import { fixDocsView, codedView, linedMockupCode } from "../SharedView.js";
import { singleton, ofArray } from "../fable_modules/fable-library-js.4.17.0/List.js";
import { Interop_reactApi } from "../fable_modules/Feliz.2.7.0/Interop.fs.js";

export const QueryTableView = (() => {
    let elms;
    const children_1 = ofArray([createElement("h1", {
        className: "title is-1",
        children: "AzureTackle - QueryTable",
    }), createElement("hr", {}), (elms = ofArray([createElement("h4", {
        className: "title is-4",
        children: "Connect to your database",
    }), createElement("p", {
        dangerouslySetInnerHTML: {
            __html: "Get the connection from the environment",
        },
    }), linedMockupCode("\n            open AzureTackle\n            let connectionString() = Env.getVar \"app_db")]), createElement("div", {
        className: "content",
        children: Interop_reactApi.Children.toArray(Array.from(elms)),
    }))]);
    return createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children_1)),
    });
})();

export const code = "\n            let data =\n                connectionString()\n                |> AzureTackle.connect\n                |> AzureTackle.query\n                \"\n                SELECT * FROM Trades\n                ORDER BY timestamp desc\n                \"\n                |> AzureTackle.execute (fun read ->\n                    { Symbol = read.string \"Symbol\"\n                      Timestamp = read.dateTime \"Timestamp\"\n                      Price = read.double \"Price\"\n                      TradeSize = read.double \"TradeSize\" })\n                |> function\n                | Ok x -> x\n                | otherwise ->\n                    printfn \"error %A\" otherwise\n                    fail () ";

export const title = "AzureTackle";

export function QueryTable() {
    let elms;
    const children_1 = ofArray([(elms = singleton(codedView(title, code, QueryTableView)), createElement("div", {
        className: "content",
        children: Interop_reactApi.Children.toArray(Array.from(elms)),
    })), fixDocsView("QueryTable", false)]);
    return createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children_1)),
    });
}

//# sourceMappingURL=QueryTable.js.map
