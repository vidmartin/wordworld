
export class Letter extends React.Component {
    constructor(props) {
        super(props);

        this.cache = {
            dragUpdateIntervalId: null
        };

        this.div = React.createRef();

        this.show = this.show.bind(this);
        this.handleMouseDown = this.handleMouseDown.bind(this);
    }

    componentDidMount() {
        this.props.onMounted?.(this);
    }

    show() {
        this.setState({
            hidden: false
        });
    }

    render() {

        let scoreElement = (<span className="letter-score">{LETTER_SCORES[this.props.letter.letter]}</span>);

        if (this.props.visible == false) {
            // TODO: při vrácení písmenek udělat příjemnou animaci přímo z místa nad odpovídajícím políčkem na hracím poli??

            return (
                <div className="letter smooth-slide"
                    ref={this.div}
                    style={{ visibility: "hidden", opacity: 0, transform: "translateY(30%)" }}>
                    {this.props.letter.letter.toUpperCase()}
                    {scoreElement}
                </div>
            );
        }

        if (this.props.dragging == true) {
            let parentClientRect = this.div.current.parentNode.getBoundingClientRect();
            let posX = this.props.posX - parentClientRect.left;
            let posY = this.props.posY - parentClientRect.top;

            return (
                <div className="letter"
                    ref={this.div}
                    style={{ /* position: "fixed", */ left: `${/* this.props.posX */ posX}px`, top: `${/* this.props.posY */ posY}px`, opacity: 0.5 }} >
                    {this.props.letter.letter.toUpperCase()}
                    {scoreElement}
                </div>
            );
        }

        if (this.props.hidden) {
            // console.log(`letter rendered as hidden (letter: ${this.props.letter}, key: ${this.props.id}, index: ${this.props.index})`);

            return (
                <div className="letter smooth-slide v-center"
                    ref={this.div}
                    style={{ left: `${this.props.hiddenLeft}`, opacity: 0 }}
                    onMouseDown={this.handleMouseDown}>
                    {this.props.letter.letter.toUpperCase()}
                    {scoreElement}
                </div>
            );
        }

        // console.log(`letter rendered normally (letter: ${this.props.letter}, key: ${this.props.id}, index: ${this.props.index})`);

        return (
            <div className="letter smooth-slide v-center"
                ref={this.div}
                style={{ left: `${this.props.index * 2}cm` }}
                onMouseDown={this.handleMouseDown} >
                {this.props.letter.letter.toUpperCase()}
                {scoreElement}
            </div>
        );
    }

    handleMouseDown(e) {
        this.props.onMouseDown?.(e, this);
    }
}