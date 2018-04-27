var baseUrl = $('#last_comments').data('url');
var croatianDateTimeFormat = new Intl.DateTimeFormat("hr-HR", { year: "numeric" , month: "2-digit", day: "2-digit", hour: "2-digit", minute: "2-digit" }).format;

class CommentSelector extends React.Component {
    constructor(props) {
        super(props);

        this.selectOption = this.selectOption.bind(this);
    }

    selectOption (event) {
        event.preventDefault();
        this.props.onCommentSelected(parseInt(event.target.text,10));        
    }

    render() {
        return (
            <div className="dropdown">
                <button className="btn btn-default btn-sm dropdown-toggle pull-right" type="button" data-toggle="dropdown">
                    {this.props.selectedOption} comments <span className="caret"></span>
                </button>
                <ul className="dropdown-menu pull-right" >
                    <li><a href="#" onClick={this.selectOption}>20</a></li>
                    <li><a href="#" onClick={this.selectOption}>50</a></li>
                    <li><a href="#" onClick={this.selectOption}>100</a></li>
                </ul>
            </div>
        );
    }
};

class CommentResults extends React.Component {
    render() {
        var commentNodes = this.props.comments.map(function(comment){
            var localDate = croatianDateTimeFormat(new Date(comment.CommentDate));

            return (
                <li className="media" key={comment.Id} >
                    <div className="pull-left">                        
                        <span className="mega-octicon octicon-comment-discussion"></span>                        
                    </div>
                    <div className="media-body">
                        <h6><strong>{comment.CommentatorName}</strong> {comment.Text}</h6>                        
                        <div><a href={baseUrl + 'Home/Details/'+comment.DefectId}><small>#{comment.DefectId} - {comment.DefectSummary}</small></a></div>
                        <div><small>{localDate}</small></div>
                    </div>
                </li>
            );
        });

        return (
            <ul>
                {commentNodes}
            </ul>
        );
    }
}

class LastComments extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            numberOfComments: 20,
            results: []
        };

        this.loadData = this.loadData.bind(this);
        this.handleCommentSelected = this.handleCommentSelected.bind(this);
    }

    handleCommentSelected(commentNo) {
        this.setState({ numberOfComments: commentNo}, function(){
            this.loadData();
        });        
    }

    componentDidMount() {
        this.loadData();
    }

    loadData() {
        $.ajax({
            dataType: "json",
            url: baseUrl + "api/home/GetLastComments",
            data: { numberOfRecords: this.state.numberOfComments },
            success: function (data) {
                this.setState({results: data});
            }.bind(this)
        });
    }

    render () {
        return (
            <div>
                <CommentSelector onCommentSelected={this.handleCommentSelected} selectedOption={this.state.numberOfComments} />
                <CommentResults comments={this.state.results} />
            </div>
        );
    }
};

ReactDOM.render(
    <LastComments />,
    document.getElementById('last_comments')
);