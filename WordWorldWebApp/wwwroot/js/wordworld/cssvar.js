
let docStyle = getComputedStyle(document.documentElement);

export function cssVar(name) {   
    return {
        name: name,
        get: function () {
            return docStyle.getPropertyValue(this.name);
        },
        set: function (value) {
            docStyle.setProperty(this.name, value);
        }
    };
}