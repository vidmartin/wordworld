
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
                <h2>Your joker is ambiguous!</h2>
                <div>Which one of these words did you mean:</div>
                <div className="scrollable-piece">{words}</div>
                <br />
                <a className="word-option" style={{ color: 'red' }} onClick={() => this.props.onCancel?.()}>cancel</a>
            </div>
        );
    }
}