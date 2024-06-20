import { ProgramModule_mkProgram, ProgramModule_run } from "./fable_modules/Fable.Elmish.4.2.0/program.fs.js";
import { Program_withReactSynchronous } from "./fable_modules/Fable.Elmish.React.4.0.0/react.fs.js";
import { AppView, update, init } from "./View.js";
import { createElement } from "react";

ProgramModule_run(Program_withReactSynchronous("safer-app", ProgramModule_mkProgram(init, update, (state_1, dispatch) => createElement(AppView, {
    state: state_1,
    dispatch: dispatch,
}))));

