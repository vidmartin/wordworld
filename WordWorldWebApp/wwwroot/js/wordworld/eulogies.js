
import { choice } from "./choice.js";

export function getCelebratoryStatementFromScore(deltaScore) {
    console.log(`getCelebratoryStatementFromScore(${deltaScore})`);

    switch (LANGUAGE.toLowerCase()) {
        case "czech":
            if (deltaScore < 0) {
                throw "that is not a score worth celebrating";
            } else if (deltaScore < 30) {
                return choice(["OK", "Dobrá", "Tak jo"]);
            } else if (deltaScore < 60) {
                return choice(["Ujde to", "Skoro dobré!", "Respektovatelné!"]);
            } else if (deltaScore < 120) {
                return choice(["To fajné!", "Vskutku dobré!", "Dobrá práce!"]);
            } else if (deltaScore < 240) {
                return choice(["Skvělé!", "Paráda!", "Velmi dobré!"]);
            } else {
                return choice(["Skvostné!", "Úžasné!", "Výjimečné!"])
            }
        default:
            if (deltaScore < 0) {
                throw "that is not a score worth celebrating";
            } else if (deltaScore < 30) {
                return choice(["OK", "Acceptable", "Aight"]);
            } else if (deltaScore < 60) {
                return choice(["Not bad", "Almost good!", "Respectable"]);
            } else if (deltaScore < 120) {
                return choice(["Very nice!", "Good indeed!", "Good job!"]);
            } else if (deltaScore < 240) {
                return choice(["Great!", "Awesome!", "Very good!"]);
            } else {
                return choice(["Superb!", "Beautiful!", "Extraordinary!"])
            }
    }
}