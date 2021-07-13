
import { localize } from "./localizer.js";

export class AmbiguousJokerResolvePopup extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        let words = this.props.words.map((word, i) => (
            <div key={i}>
                <a className="word-option" onClick={() => this.props.onWordClick?.(word)}>{word}</a>
            </div>
        ));

        return (
            <div className="popup">
                <h2>{localize("ambiguous_joker_popup.header")}</h2>
                <div>{localize("ambiguous_joker_popup.text")}</div>
                <div className="scrollable-piece">{words}</div>
                <br />
                <a className="word-option" style={{ color: 'red' }} onClick={() => this.props.onCancel?.()}>{localize("cancel")}</a>
            </div>
        );
    }
}