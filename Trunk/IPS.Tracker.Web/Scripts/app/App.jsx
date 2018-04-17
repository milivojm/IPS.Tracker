var taskList = []

var releaseNumber = '';
var releaseDate = '';

var submitDisabled = true;

var selectedOption = '';

class App extends React.Component {

  constructor(props) {
    super(props);

    this.state = {
        taskList,
        releaseNumber,
        releaseDate,
        selectedOption,       
    };

    this.handleAddTask = this.handleAddTask.bind(this);    
    this.handleSubmit = this.handleSubmit.bind(this);
    this.handleAddReleaseNumber = this.handleAddReleaseNumber.bind(this);
    this.handleAddReleaseDate = this.handleAddReleaseDate.bind(this);
    this.handleSelectTest = this.handleSelectTest.bind(this);
  }

    handleAddReleaseNumber(number) {
        if (number.releaseNumber !== "") {
            submitDisabled = false;
        }
        else {
            submitDisabled = true;
        }

        this.setState({
            releaseNumber: number
        })
    }
    
    handleAddReleaseDate(date) {
        this.setState({
            releaseDate: date
        })
    }

    handleSelectTest(task) {        
        this.setState({            
            taskList: this.state.taskList.concat(task)
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
    var data = {
        ReleaseListDefectId: this.state.taskList,
        ReleaseNo: this.state.releaseNumber.releaseNumber,
        EstDateOfRelease: this.state.releaseDate.releaseDate
    }    

    var arr = [];
    
    for (var i = 0; i < this.state.taskList.length; i++) {        
        arr[i] = this.state.taskList[i].taskNo;        
    }
    
    data.ReleaseListDefectId = arr;
          
    var url = "/Home/NewRelease";            
        
    $.ajax({        
        method: "POST",
        url: url,
        dataType: 'json',
        data: data,
        cache: false,
        success: function (data) {
            this.setState({ data: data });
        }.bind(this),
        error: function (xhr, status, err) {
            console.error(this.props.url, status, err.toString());
        }.bind(this)
    });                   
  }

  render() {
    return (
      <div className="container">
        <div className="form-horizontal">
          <div className="form-group">
            <h2>New App Release</h2>
              <hr />
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
            <SelectTest onSelectTest={this.handleSelectTest}></SelectTest>
          </div>
          </div>          

            <div className="form-horizontal">
                <div className="form-group">
                    <div className="label-control col-md-2"></div>
                    <div className="col-md-4-offset-0">
                        <div className="form-group">
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
            </div>                      
                    
            <div className="form-horizontal">
                <div className="form-group">
                    <div className="col-md-2"></div>
                         <div className="col-md-4">
                            <form onSubmit={this.handleSubmit}>
                              <label>
                                <input type="submit" value="Save" className="btn btn-danger" disabled={submitDisabled}></input>
                              </label>
                            </form>
                        </div>
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

        this.handleChange = this.handleChange.bind(this);
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
            <label className="control-label col-md-2">Release version*</label>
            <div className="col-md-4">
              <form>
                <input className="form-control" type="text" value={this.state.releaseNumber} onChange={this.handleChange}  placeholder="*Required*"></input>
              </form>
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
            <label className="control-label col-md-2">Release date*</label>
            <div className="col-md-4">
              <form>
                <input className="form-control" type="date" value={this.state.releaseDate} onChange={this.handleChange}></input>
              </form>
            </div>
          </div>
      )
  }
}

class SelectTest extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            taskNo: ''            
        }

        this.handleChange = this.handleChange.bind(this);
    }
    
    handleChange(taskNo) {      

        this.setState({
            taskNo: taskNo.value
            }, () => {
            this.props.onSelectTest(this.state);
            })        
    }

    componentDidMount() {        
        var url = "/Home/GetDefects";
        
        var items = [{
            value: '',
            label: ''
        }];        

        axios.get(url)
            .then(function (response) {                
                for (var i = 0; i < response.data.length; i++) {
                    items.push({
                        "value": response.data[i].Id,
                        "label": response.data[i].Id + ' - ' + response.data[i].Summary
                    })
                }
         })
        .catch(function (error) {
            console.log(error);
        });

        return items;                
    }

    render() {
        const taskNo = this.state;
        const value = taskNo && taskNo.value;
        var options = this.componentDidMount();

        return (

            <div className="form-group row">
                <label className="control-label col-md-2">Release task list*</label>
                <div className="col-md-4">
                     <Select name="form-field-name"
                             value={value}
                             onChange={this.handleChange}                       
                             options={options}/>                                                  
                </div>
            </div>         
        )
    }
}

ReactDOM.render(
    <App />, 
    document.getElementById('root')
);
