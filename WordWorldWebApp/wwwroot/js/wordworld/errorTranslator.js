
import { localize } from "./localizer.js";

export function localizeError(error) {
    return localize(`error.${error}`) ?? `[${error}]`;
}