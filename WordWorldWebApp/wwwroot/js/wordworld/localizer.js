
import { choice } from "./choice.js";

export function localize(key) {
    return DICTIONARY[key];
}

export function localizeRandom(key) {
    let regex = new RegExp(`^${key}\\.\\d+\$`);
    let values = Object.entries(DICTIONARY)
        .filter(entry => regex.test(entry[0]))
        .map(entry => entry[1]);
    return choice(values);
}