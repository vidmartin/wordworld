
var SCORE_STEP_FACTOR = 0.1

export class Score extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            currentScore: props.score,
            scale: 1
        };

        this.scoreStep = this.scoreStep.bind(this);
    }

    componentDidMount() {
        setInterval(this.scoreStep, 40);
    }

    scoreStep() {
        let step = (this.props.score - this.state.currentScore) * SCORE_STEP_FACTOR;

        this.setState({
            currentScore: this.state.currentScore + Math.ceil(step),
            scale: 1 + Math.min(step / 3, 0.2)
        });
    }

    render() {
        return (
            <div className="score" style={{ transform: `scale(${this.state.scale})` }}>{this.state.currentScore}</div>
        );
    }
}