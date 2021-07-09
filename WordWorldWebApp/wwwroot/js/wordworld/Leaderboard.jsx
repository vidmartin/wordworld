
var LEADERBOARD_ITEM_HEIGHT_CM = 0.8;

function LeaderboardItem(props) {
    return (
        <div className="leaderboard-item" style={{
            height: `${LEADERBOARD_ITEM_HEIGHT_CM}cm`,
            top: `${LEADERBOARD_ITEM_HEIGHT_CM * props.index}cm`
        }}>
            <div className="leaderboard-item-username">{props.username}</div>
            <div className="leaderboard-item-score">{props.score}</div>
        </div>
    );
}

export class Leaderboard extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            leaderboard: []
        };

        this.cache = {
            fetching: false
        };

        this.updateLeaderboard = this.updateLeaderboard.bind(this);
    }

    componentDidMount() {
        setInterval(this.updateLeaderboard, 500);
    }

    updateLeaderboard() {
        if (this.cache.fetching == true) {
            return;
        }

        this.cache.fetching = true;

        fetch(`/game/leaderboard?token=${PLAYER_TOKEN}`)
            .then(response => {
                if (response.ok == false) {
                    throw `fetching leaderboard failed - error code ${response.status}`;
                }

                return response.json();
            })
            .then(json => {
                if (json.status != "ok") {
                    throw `fetching leaderboard failed - backend not happy (${json.status})`;
                }

                this.setState({
                    leaderboard: json.data.players
                });
            })
            .finally(() => {
                this.cache.fetching = false;
            });
    }

    render() {
        return (
            <div className="leaderboard">
                <div className="leaderboard-header">Leaderboard</div>
                <div className="leaderboard-content">
                    {
                        this.state.leaderboard.map((item, index) => (
                            <LeaderboardItem key={item.username}
                                username={item.username}
                                score={item.score}
                                index={index} />
                        ))
                    }
                </div>
            </div>
        );
    }
}