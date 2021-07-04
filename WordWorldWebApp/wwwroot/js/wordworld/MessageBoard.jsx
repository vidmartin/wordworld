
export class MessageBoard extends React.Component {

    constructor(props) {
        super(props);

        MessageBoard.boardDict[props.id ?? "default"] = this;

        this.state = {
            messages: {}
        };

        // messages look like this: { classes: [additional css classes], message: [text content] }

        this.cache = {
            counter: 0
        };
    }    

    render() {
        let messageElements = Object.keys(this.state.messages).map(key => (
            <div className={`message ${this.state.messages[key].classes}`} key={key}>
                {this.state.messages[key].message}
            </div>
        ));

        return (
            <div className="message-board">
                {messageElements}
            </div>
        );
    }

    showMessage(message, classes, timeout) {
        let key = ++this.cache.counter;

        this.setState({
            messages: {
                ...this.state.messages,
                [key]: {
                    classes: classes,
                    message: message
                }
            }
        });

        setTimeout(() => {
            delete this.state.messages[key];
            this.forceUpdate();
        }, timeout);
    }

    static boardDict = {};

    static writeOk(msg, boardId = "default") {
        MessageBoard.boardDict[boardId].showMessage(msg, "ok", 2000);
    }

    static writeError(msg, boardId = "default") {
        MessageBoard.boardDict[boardId].showMessage(msg, "nok", 2000);
    }

}