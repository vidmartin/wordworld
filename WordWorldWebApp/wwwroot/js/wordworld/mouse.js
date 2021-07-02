
let mousePos = { x: 0, y: 0 };

window.addEventListener("mousemove", ev => mousePos = {
    x: ev.clientX, y: ev.clientY
});

export function getMousePos() {
    return mousePos;
}