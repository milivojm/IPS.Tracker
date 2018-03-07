var taskList = [
  {
      taskNo: '1'
  },
  {
      taskNo: '45'
  },
  {
      taskNo: '564'
  }
]

var releaseNumber = '';
var releaseDate = '';

class App extends React.Component {

  constructor(props) {
    super(props);

    this.state = {
        taskList,
        releaseNumber,
        releaseDate
    };

    this.handleAddTask = this.handleAddTask.bind(this);    
    this.handleSubmit = this.handleSubmit.bind(this);
    this.handleAddReleaseNumber = this.handleAddReleaseNumber.bind(this);
    this.handleAddReleaseDate = this.handleAddReleaseDate.bind(this);
  }

    handleAddReleaseNumber(number) {
        this.setState({
            releaseNumber: number
        })
    }
    
    handleAddReleaseDate(date) {
        this.setState({
            releaseDate: date
        })
    }

  handleRemoveTask(index) {
    this.setState({
      taskList: this.state.taskList.filter(function(e,i) {
        return i !== index;
      })
    })
  }


  handleAddTask(task) {      
      this.setState({
          taskList: this.state.taskList.concat(task)
      })
  }  
 

  handleSubmit(event) {
    console.log("slanje podataka" + this.state);
    event.preventDefault();
  }

  render() {
    return (
      <div className="container">
        <div className="form-horizontal">
          <div className="form-group">
            <h2>New App Release</h2>
          </div>
        </div>
        <div className="form-horizontal">
          <div className="form-group">
            <ReleaseNumber onAddReleaseNumber={this.handleAddReleaseNumber}></ReleaseNumber>
          </div>
        </div>
          <div className="form-horizontal">
          <div className="form-group">
            <ReleaseDate onAddReleaseDate={this.handleAddReleaseDate}></ReleaseDate>
          </div>
          </div>
          <div className="form-horizontal">
          <div className="form-group">
            <TaskInput onAddTask={this.handleAddTask}></TaskInput>
            <div className="col-md-4-offset-0">
              <ul className="list-group col-md-4">
                  {this.state.taskList.map((task, index) =>
                  <li className="list-group-item" key={index}>{task.taskNo}
                    <span className="pull-right">
                      <button className="btn btn-xs btn-danger" onClick={this.handleRemoveTask.bind(this, index)}><span className="glyphicon glyphicon-trash"></span> Delete</button>
                    </span>
                  </li>
                  )}
              </ul>
            </div>
          </div>
          </div>


        <div className="form-horizontal">
          <div className="form-group">
            <form onSubmit={this.handleSubmit}>
              <label>
                <input type="submit" value="Save" className="btn btn-danger"></input>
              </label>
            </form>
          </div>
        </div>
      </div>
    );
  }
}

class ReleaseNumber extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            releaseNumber: ''
        }
    }

    handleChange(event) {
        this.setState({
            releaseNumber: event.target.value
        }, () => {
            this.props.onAddReleaseNumber(this.state);
        })
    }

    render() {
        return (
          <div className="form-group row">
            <label className="col-md-2">Release number</label>
            <div className="col-md-2">
              <form>
                <input className="form-control" type="text" value={this.state.releaseNumber} onChange={this.handleChange}></input>
              </form>
            </div>
          </div>
      )
  }
}

class TaskInput extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            taskNo: ''
        }

        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    handleChange(event) {
        this.setState({
            taskNo: event.target.value
        })
    }

    handleSubmit(event) {
        event.preventDefault();
        this.props.onAddTask(this.state);
        this.setState({
            taskNo: ''
        })
    }

    render() {
        return (
          <div>
          <div className="form-group row">
            <label className="col-sm-2 col-form-label">
                <h4>Release task list</h4>
            </label>
          </div>
          <div className="form-group row">
            <div className="col-md-6">
              <form onSubmit={this.handleSubmit}>
                <div className="form-group">
                  <div className="col-md-4">
                    <input name="taskNo"
        type="text"
        className="form-control"
        id="inputTaskNo"
        value={this.state.taskNo}
        onChange={this.handleChange}
        placeholder="Task Number">
</input>
</div>
<div className="col-md-4">
  <button type="submit" className="btn btn-success">Add Task</button>
</div>
</div>
</form>
</div>
</div>
</div>
      )
  }
}

class ReleaseDate extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            releaseDate: ''
        }

        this.handleChange = this.handleChange.bind(this);
    }

    handleChange(event) {
        this.setState({
            releaseDate: event.target.value
        }, () => {
            this.props.onAddReleaseDate(this.state);
        })
    }

    render() {
        return (
          <div className="form-group row">
            <label className="col-md-2">Release date</label>
            <div className="col-md-2">
              <form>
                <input className="form-control" type="date" value={this.state.releaseDate} onChange={this.handleChange}></input>
              </form>
            </div>
          </div>
      )
  }
}

ReactDOM.render(
    <App />,
    document.getElementById('root')
);
