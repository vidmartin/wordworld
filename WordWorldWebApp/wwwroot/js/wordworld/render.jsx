
import { GameContainer } from "./GameContainer.jsx";

export function render() {
    ReactDOM.render(<GameContainer />, document.querySelector("#react-target"));
}