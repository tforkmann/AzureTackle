import { Program_Internal_withReactSynchronousUsing } from "./fable_modules/Fable.Elmish.React.4.0.0/react.fs.js";
import { lazyView2With } from "./fable_modules/Fable.Elmish.HMR.7.0.0/common.fs.js";
import { uncurry2 } from "./fable_modules/fable-library-js.4.17.0/Util.js";
import { ProgramModule_mkProgram, ProgramModule_withConsoleTrace } from "./fable_modules/Fable.Elmish.4.2.0/program.fs.js";
import { State_$reflection, AppView, update, init } from "./View.js";
import { createElement } from "react";
import { Program_withDebuggerUsing, Debugger_showWarning, Debugger_showError } from "./fable_modules/Fable.Elmish.Debugger.4.0.0/debugger.fs.js";
import { empty as empty_1, cons, singleton, ofArray } from "./fable_modules/fable-library-js.4.17.0/List.js";
import { newGuid } from "./fable_modules/fable-library-js.4.17.0/Guid.js";
import { add } from "./fable_modules/fable-library-js.4.17.0/Map.js";
import { Auto_generateBoxedEncoder_437914C6, uint64, int64, decimal } from "./fable_modules/Thoth.Json.10.2.0/Encode.fs.js";
import { Auto_generateBoxedDecoder_Z6670B51, uint64 as uint64_1, int64 as int64_1, decimal as decimal_1 } from "./fable_modules/Thoth.Json.10.2.0/Decode.fs.js";
import { empty } from "./fable_modules/Thoth.Json.10.2.0/Extra.fs.js";
import { ExtraCoders } from "./fable_modules/Thoth.Json.10.2.0/Types.fs.js";
import { fromValue } from "./fable_modules/Thoth.Json.10.2.0/Decode.fs.js";
import { Debugger_ConnectionOptions } from "./fable_modules/Fable.Elmish.Debugger.4.0.0/debugger.fs.js";
import { getCaseFields, getCaseName as getCaseName_1, isUnion } from "./fable_modules/fable-library-js.4.17.0/Reflection.js";
import { item } from "./fable_modules/fable-library-js.4.17.0/Array.js";
import { join } from "./fable_modules/fable-library-js.4.17.0/String.js";
import { Options$1 } from "./fable_modules/Fable.Elmish.Debugger.4.0.0/Fable.Import.RemoteDev.fs.js";
import { connectViaExtension } from "remotedev";
import { FSharpRef } from "./fable_modules/fable-library-js.4.17.0/Types.js";
import { defaultOf } from "./fable_modules/fable-library-js.4.17.0/Util.js";
import { current as current_2 } from "./fable_modules/Fable.Elmish.HMR.7.0.0/Bundler.fs.js";
import { Internal_saveState, Internal_tryRestoreState } from "./fable_modules/Fable.Elmish.HMR.7.0.0/hmr.fs.js";
import { Cmd_map, Cmd_none } from "./fable_modules/Fable.Elmish.4.2.0/cmd.fs.js";
import { Msg$1 } from "./fable_modules/Fable.Elmish.HMR.7.0.0/hmr.fs.js";
import { Sub_map, Sub_batch } from "./fable_modules/Fable.Elmish.4.2.0/sub.fs.js";
import { ProgramModule_map, ProgramModule_runWith } from "./fable_modules/Fable.Elmish.4.2.0/program.fs.js";
import "../index.css";


(function () {
    let program_5;
    const program_4 = Program_Internal_withReactSynchronousUsing((equal, view, state_2, dispatch_1) => lazyView2With(uncurry2(equal), uncurry2(view), state_2, dispatch_1), "safer-app", ProgramModule_withConsoleTrace(ProgramModule_mkProgram(init, update, (state_1, dispatch) => createElement(AppView, {
        state: state_1,
        dispatch: dispatch,
    }))));
    try {
        let patternInput;
        try {
            let coders;
            let extra_2_1;
            const extra_1_1 = new ExtraCoders((() => {
                let copyOfStruct = newGuid();
                return copyOfStruct;
            })(), add("System.Decimal", [decimal, (path) => ((value_1) => decimal_1(path, value_1))], empty.Coders));
            extra_2_1 = (new ExtraCoders((() => {
                let copyOfStruct_1 = newGuid();
                return copyOfStruct_1;
            })(), add("System.Int64", [int64, int64_1], extra_1_1.Coders)));
            coders = (new ExtraCoders((() => {
                let copyOfStruct_2 = newGuid();
                return copyOfStruct_2;
            })(), add("System.UInt64", [uint64, uint64_1], extra_2_1.Coders)));
            const encoder_3 = Auto_generateBoxedEncoder_437914C6(State_$reflection(), undefined, coders, undefined);
            const decoder_3 = Auto_generateBoxedDecoder_Z6670B51(State_$reflection(), undefined, coders);
            const deflate = (x) => {
                try {
                    return encoder_3(x);
                }
                catch (er) {
                    Debugger_showWarning(singleton(er.message));
                    return x;
                }
            };
            const inflate = (x_1) => {
                const matchValue = fromValue("$", uncurry2(decoder_3), x_1);
                if (matchValue.tag === 1) {
                    const er_1 = matchValue.fields[0];
                    throw new Error(er_1);
                }
                else {
                    const x_2 = matchValue.fields[0];
                    return x_2;
                }
            };
            patternInput = [deflate, inflate];
        }
        catch (er_2) {
            Debugger_showWarning(singleton(er_2.message));
            patternInput = [(value_7) => value_7, (_arg) => {
                throw new Error("Cannot inflate model");
            }];
        }
        const inflater = patternInput[1];
        const deflater = patternInput[0];
        let connection;
        const opt = new Debugger_ConnectionOptions(0, []);
        const makeMsgObj = (tupledArg) => {
            const case$ = tupledArg[0];
            const fields = tupledArg[1];
            return {
                type: case$,
                msg: fields,
            };
        };
        const getCase = (x_3) => {
            if (isUnion(x_3)) {
                const getCaseName = (acc_mut, x_1_1_mut) => {
                    getCaseName:
                    while (true) {
                        const acc = acc_mut, x_1_1 = x_1_1_mut;
                        const acc_1 = cons(getCaseName_1(x_1_1), acc);
                        const fields_1 = getCaseFields(x_1_1);
                        if ((fields_1.length === 1) && isUnion(item(0, fields_1))) {
                            acc_mut = acc_1;
                            x_1_1_mut = item(0, fields_1);
                            continue getCaseName;
                        }
                        else {
                            return makeMsgObj([join("/", acc_1), fields_1]);
                        }
                        break;
                    }
                };
                return getCaseName(empty_1(), x_3);
            }
            else {
                return makeMsgObj(["NOT-AN-F#-UNION", x_3]);
            }
        };
        const fallback = new Options$1(true, 443, "remotedev.io", true, getCase);
        connection = connectViaExtension((opt.tag === 1) ? (() => {
            const port = opt.fields[1] | 0;
            const address = opt.fields[0];
            const inputRecord_1 = fallback;
            return new Options$1(inputRecord_1.remote, port, address, false, inputRecord_1.getActionType);
        })() : ((opt.tag === 2) ? (() => {
            const port_1 = opt.fields[1] | 0;
            const address_1 = opt.fields[0];
            const inputRecord_2 = fallback;
            return new Options$1(inputRecord_2.remote, port_1, address_1, inputRecord_2.secure, inputRecord_2.getActionType);
        })() : (new Options$1(false, 8000, "localhost", false, fallback.getActionType))));
        program_5 = Program_withDebuggerUsing(deflater, inflater, connection, program_4);
    }
    catch (ex) {
        Debugger_showError(ofArray(["Unable to connect to the monitor, continuing w/o debugger", ex.message]));
        program_5 = program_4;
    }
    const hmrState = new FSharpRef(defaultOf());
    if (current_2 == null) {
    }
    else {
        const current = current_2;
        window.Elmish_HMR_Count = ((window.Elmish_HMR_Count == null) ? 0 : (window.Elmish_HMR_Count + 1));
        let hmrDataObject;
        switch (current.tag) {
            case 1: {
                ((import.meta.webpackHot /* If error see https://github.com/elmish/hmr/issues/35 */)).accept();
                hmrDataObject = ((import.meta.webpackHot /* If error see https://github.com/elmish/hmr/issues/35 */)).data;
                break;
            }
            case 2: {
                (module.hot).accept();
                hmrDataObject = (module.hot).data;
                break;
            }
            default: {
                import.meta.hot.accept();
                hmrDataObject = (import.meta.hot.data);
            }
        }
        Internal_tryRestoreState(hmrState, hmrDataObject);
    }
    const mapUpdate = (userUpdate, msg_1, model) => {
        let patternInput_1;
        if (msg_1.tag === 1) {
            patternInput_1 = [model, Cmd_none()];
        }
        else {
            const userMsg = msg_1.fields[0];
            patternInput_1 = userUpdate(userMsg)(model);
        }
        const newModel = patternInput_1[0];
        const cmd = patternInput_1[1];
        hmrState.contents = newModel;
        return [newModel, Cmd_map((Item) => (new Msg$1(0, [Item])), cmd)];
    };
    const createModel = (tupledArg_1) => {
        const model_1 = tupledArg_1[0];
        const cmd_1 = tupledArg_1[1];
        return [model_1, cmd_1];
    };
    const mapInit = (userInit, args) => {
        if (hmrState.contents == null) {
            const patternInput_1_1 = userInit(args);
            const userModel = patternInput_1_1[0];
            const userCmd = patternInput_1_1[1];
            return [userModel, Cmd_map((Item_1) => (new Msg$1(0, [Item_1])), userCmd)];
        }
        else {
            return [hmrState.contents, Cmd_none()];
        }
    };
    const mapSetState = (userSetState, userModel_1, dispatch_2) => userSetState(userModel_1)((arg_2) => dispatch_2(new Msg$1(0, [arg_2])));
    let hmrSubscription;
    const handler = (dispatch_1_1) => {
        if (current_2 == null) {
        }
        else {
            const current_1 = current_2;
            switch (current_1.tag) {
                case 1: {
                    ((import.meta.webpackHot /* If error see https://github.com/elmish/hmr/issues/35 */)).dispose((data_1) => {
                        Internal_saveState(data_1, hmrState.contents);
                        dispatch_1_1(new Msg$1(1, []));
                    });
                    break;
                }
                case 2: {
                    (module.hot).dispose((data_2) => {
                        Internal_saveState(data_2, hmrState.contents);
                        dispatch_1_1(new Msg$1(1, []));
                    });
                    break;
                }
                default:
                    import.meta.hot.dispose((data) => {
                        Internal_saveState(data, hmrState.contents);
                        dispatch_1_1(new Msg$1(1, []));
                    });
            }
        }
        return {
            Dispose() {
            },
        };
    };
    hmrSubscription = singleton([singleton("Hmr"), handler]);
    const mapSubscribe = (subscribe, model_2) => Sub_batch(ofArray([Sub_map("HmrUser", (Item_3) => (new Msg$1(0, [Item_3])), subscribe(model_2)), hmrSubscription]));
    const mapView = (userView, model_3, dispatch_2_1) => userView(model_3)((arg_3) => dispatch_2_1(new Msg$1(0, [arg_3])));
    const mapTermination = (tupledArg_1_1) => {
        const predicate = tupledArg_1_1[0];
        const terminate = tupledArg_1_1[1];
        const mapPredicate = (_arg_1) => {
            if (_arg_1.tag === 1) {
                return true;
            }
            else {
                const msg_1_1 = _arg_1.fields[0];
                return predicate(msg_1_1);
            }
        };
        return [mapPredicate, terminate];
    };
    ProgramModule_runWith(undefined, ProgramModule_map(mapInit, mapUpdate, mapView, mapSetState, mapSubscribe, mapTermination, program_5));
})();

//# sourceMappingURL=App.js.map
