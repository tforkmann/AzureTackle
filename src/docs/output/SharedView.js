import { printf, toText, split } from "./fable_modules/fable-library-js.4.17.0/String.js";
import { empty, singleton, append, delay, toList, mapIndexed } from "./fable_modules/fable-library-js.4.17.0/Seq.js";
import { createElement } from "react";
import { int32ToString, createObj } from "./fable_modules/fable-library-js.4.17.0/Util.js";
import { Interop_reactApi } from "./fable_modules/Feliz.2.7.0/Interop.fs.js";
import { ofArray, singleton as singleton_1 } from "./fable_modules/fable-library-js.4.17.0/List.js";
import { Helpers_combineClasses } from "./fable_modules/Feliz.DaisyUI.4.2.1/DaisyUI.fs.js";

export function linedMockupCode(code) {
    const lines = split(code, ["\n"], undefined, 0);
    const children = mapIndexed((i, l) => createElement("pre", createObj(toList(delay(() => append(singleton(["data-prefix", int32ToString(i + 1)]), delay(() => append((l.indexOf("// ") >= 0) ? singleton(["className", "text-warning"]) : empty(), delay(() => {
        let elems;
        return singleton((elems = [createElement("code", {
            children: [l],
        })], ["children", Interop_reactApi.Children.toArray(Array.from(elems))]));
    })))))))), lines);
    return createElement("div", {
        className: "mockup-code",
        children: Interop_reactApi.Children.toArray(Array.from(children)),
    });
}

export function codedView(title, code, example) {
    let elm_4, elm_6;
    const elm = ofArray([createElement("div", {
        className: "description",
        children: Interop_reactApi.Children.toArray([title]),
    }), (elm_4 = ofArray([(elm_6 = singleton_1(linedMockupCode(code)), createElement("div", {
        className: "grid flex-1 h-full",
        children: Interop_reactApi.Children.toArray(Array.from(elm_6)),
    })), createElement("div", createObj(Helpers_combineClasses("divider", ofArray([["className", "divider-horizontal"], ["children", "👉"], ["className", "text-neutral"], ["className", "after:bg-opacity-30 before:bg-opacity-30"]])))), createElement("div", {
        className: "grid flex-1",
        children: Interop_reactApi.Children.toArray([example]),
    })]), createElement("div", {
        className: "flex flex-row w-full",
        children: Interop_reactApi.Children.toArray(Array.from(elm_4)),
    }))]);
    return createElement("div", {
        className: "mb-10",
        children: Interop_reactApi.Children.toArray(Array.from(elm)),
    });
}

export function codedWithPictureView(title, code, imageScr) {
    let elm_4, elm_6, elm_8;
    const elm = ofArray([createElement("div", {
        className: "description",
        children: Interop_reactApi.Children.toArray([title]),
    }), (elm_4 = ofArray([(elm_6 = singleton_1(linedMockupCode(code)), createElement("div", {
        className: "grid flex-1 h-full",
        children: Interop_reactApi.Children.toArray(Array.from(elm_6)),
    })), createElement("div", createObj(Helpers_combineClasses("divider", ofArray([["className", "divider-horizontal"], ["children", "👉"], ["className", "text-neutral"], ["className", "after:bg-opacity-30 before:bg-opacity-30"]])))), (elm_8 = singleton_1(createElement("img", {
        src: imageScr,
    })), createElement("div", {
        className: "grid flex-1",
        children: Interop_reactApi.Children.toArray(Array.from(elm_8)),
    }))]), createElement("div", {
        className: "flex flex-row w-full",
        children: Interop_reactApi.Children.toArray(Array.from(elm_4)),
    }))]);
    return createElement("div", {
        className: "mb-10",
        children: Interop_reactApi.Children.toArray(Array.from(elm)),
    });
}

export function codedWithTextExampleView(title, code, example) {
    let elm_4, elm_6, elm_8;
    const elm = ofArray([createElement("div", {
        className: "description",
        children: Interop_reactApi.Children.toArray([title]),
    }), (elm_4 = ofArray([(elm_6 = singleton_1(linedMockupCode(code)), createElement("div", {
        className: "grid flex-1 h-full",
        children: Interop_reactApi.Children.toArray(Array.from(elm_6)),
    })), createElement("div", createObj(Helpers_combineClasses("divider", ofArray([["className", "divider-horizontal"], ["children", "👉"], ["className", "text-neutral"], ["className", "after:bg-opacity-30 before:bg-opacity-30"]])))), (elm_8 = singleton_1(linedMockupCode(example)), createElement("div", {
        className: "grid flex-1 h-full",
        children: Interop_reactApi.Children.toArray(Array.from(elm_8)),
    }))]), createElement("div", {
        className: "flex flex-row w-full",
        children: Interop_reactApi.Children.toArray(Array.from(elm_4)),
    }))]);
    return createElement("div", {
        className: "mb-10",
        children: Interop_reactApi.Children.toArray(Array.from(elm)),
    });
}

export function codedNoExampleView(title, code) {
    let elm_4, elm_6;
    const elm = ofArray([createElement("div", {
        className: "description",
        children: Interop_reactApi.Children.toArray([title]),
    }), (elm_4 = singleton_1((elm_6 = singleton_1(linedMockupCode(code)), createElement("div", {
        className: "grid flex-1 h-full",
        children: Interop_reactApi.Children.toArray(Array.from(elm_6)),
    }))), createElement("div", {
        className: "flex flex-row w-full",
        children: Interop_reactApi.Children.toArray(Array.from(elm_4)),
    }))]);
    return createElement("div", {
        className: "mb-10",
        children: Interop_reactApi.Children.toArray(Array.from(elm)),
    });
}

export function fixDocsView(fileName, client) {
    const children = singleton_1(createElement("a", {
        href: toText(printf("https://github.com/tforkmann/Feliz.ChartJS/blob/main/src/Docs/views/Feliz.ChartJS/%s.fs"))(fileName),
        children: ("Fix docs file " + fileName) + " here",
    }));
    return createElement("div", {
        children: Interop_reactApi.Children.toArray(Array.from(children)),
    });
}

//# sourceMappingURL=SharedView.js.map
