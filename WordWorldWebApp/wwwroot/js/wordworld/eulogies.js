
import { localizeRandom } from "./localizer.js";

export function getCelebratoryStatementFromScore(deltaScore) {
    console.log(`getCelebratoryStatementFromScore(${deltaScore})`);

    if (deltaScore < 0) {
        throw "that is not a score worth celebrating";
    } else if (deltaScore < 30) {
        return localizeRandom("score_level_1");
    } else if (deltaScore < 60) {
        return localizeRandom("score_level_2");
    } else if (deltaScore < 120) {
        return localizeRandom("score_level_3");
    } else if (deltaScore < 240) {
        return localizeRandom("score_level_4");
    } else {
        return localizeRandom("score_level_5");
    }
}