
export function choice(values) {
    let index = Math.floor(Math.random() * values.length);
    return values[index];
}