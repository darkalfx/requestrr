import React from "react";
import { Oval } from 'react-loader-spinner'
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { loadRadarrProfiles } from "../../../store/actions/RadarrClientActions"
import { loadRadarrRootPaths } from "../../../store/actions/RadarrClientActions"
import { loadRadarrTags } from "../../../store/actions/RadarrClientActions"
import { setRadarrCategory } from "../../../store/actions/RadarrClientActions"
import { removeRadarrCategory } from "../../../store/actions/RadarrClientActions"
import ValidatedTextbox from "../../Inputs/ValidatedTextbox"
import Dropdown from "../../Inputs/Dropdown"
import MultiDropdown from "../../Inputs/MultiDropdown"

import {
  Row,
  Col,
  Collapse
} from "reactstrap";

class RadarrCategory extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      nameErrorMessage: "",
      isNameValid: true,
      isOpen: false,
    };

    this.validateName = this.validateName.bind(this);
    this.setCategory = this.setCategory.bind(this);
    this.deleteCategory = this.deleteCategory.bind(this);
  }

  componentDidMount() {
    if (this.props.category.wasCreated) {
      this.setState({ isOpen: true });
    }

    if (this.props.canConnect) {
      this.props.loadProfiles(false);
      this.props.loadRootPaths(false);
      this.props.loadTags(false);
    }
  }

  componentDidUpdate(prevProps, prevState) {
    var previousNames = prevProps.radarr.categories.map(x => x.name);
    var currentNames = this.props.radarr.categories.map(x => x.name);

    if (!(previousNames.length === currentNames.length && currentNames.every((value, index) => previousNames[index] === value))) {
      this.validateName(this.props.category.name)
    }

    if (this.props.canConnect) {
      this.props.loadProfiles(false);
      this.props.loadRootPaths(false);
      this.props.loadTags(false);
    }

    if (prevProps.isSaving !== this.props.isSaving) {
      this.setState({
        isOpen: false,
      });
    }
  }

  validateNonEmptyString = value => {
    return /\S/.test(value);
  }

  validateName(value) {
    var state = { isNameValid: true };

    if (!/\S/.test(value)) {
      state = {
        ...state,
        nameErrorMessage: "A category name is required.",
        isNameValid: false,
      };
    }
    else if (/^[\w-]{1,32}$/.test(value)) {
      if (this.props.radarr.categories.map(x => x.id).includes(this.props.category.id) && this.props.radarr.categories.filter(c => typeof c.id !== 'undefined' && c.id !== this.props.category.id && c.name.toLowerCase().trim() === value.toLowerCase().trim()).length > 0) {
        state = {
          nameErrorMessage: "All categories must have different names.",
          isNameValid: false,
        };
      }
    }
    else {
      state = {
        nameErrorMessage: "Invalid categorie names, make sure they only contain alphanumeric characters, dashes and underscores. (No spaces, etc)",
        isNameValid: false,
      };
    }

    this.setState(state);

    return state.isNameValid;
  }
  
  setCategory(fieldChanged, data) {
    this.props.setRadarrCategory(this.props.category.id, fieldChanged, data);
  }

  deleteCategory() {
    this.setState({
      isOpen: false,
    }, () => setTimeout(() => this.props.removeRadarrCategory(this.props.category.id), 150));
  }

  render() {
    return (
      <>
        <tr class="fade show">
          <th scope="row">
            <div class="media align-items-center">
              <div class="media-body">
                <span class="name mb-0 text-sm">{this.props.category.name}</span>
              </div>
            </div>
          </th>
          <td class="text-right">
            <button onClick={() => this.setState({ isOpen: !this.state.isOpen })} disabled={this.state.isOpen && (!this.state.isNameValid || !this.props.radarr.arePathsValid || !this.props.radarr.areProfilesValid || !this.props.radarr.areTagsValid)} className="btn btn-icon btn-3 btn-info" type="button">
              <span className="btn-inner--icon"><i class="fas fa-edit"></i></span>
              <span className="btn-inner--text">Edit</span>
            </button>
          </td>
        </tr>
        <tr>
          <td className="border-0 pb-0 pt-0" colSpan="2">
            <Collapse isOpen={this.state.isOpen}>
              <div className="mb-4">
                <Row>
                  <Col lg="6">
                    <ValidatedTextbox
                      name="Category Name"
                      placeholder="Enter category name"
                      alertClassName="mt-3 text-wrap"
                      errorMessage={this.state.nameErrorMessage}
                      isSubmitted={this.props.isSubmitted || this.state.isNameValid}
                      value={this.props.category.name}
                      validation={this.validateName}
                      onChange={newName => this.setCategory("name", newName)}
                      onValidate={isValid => this.setState({ isNameValid: isValid })} />
                  </Col>
                </Row>
                <Row>
                  <Col lg="6" className="mb-4">
                    <div className="input-group-button">
                      <Dropdown
                        name="Path"
                        value={this.props.category.rootFolder}
                        items={this.props.radarr.paths.map(x => { return { name: x.path, value: x.path } })}
                        onChange={newPath => this.setCategory("rootFolder", newPath)} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadRootPaths(true)} disabled={!this.props.canConnect} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.props.radarr.isLoadingPaths ? (
                              <Oval
                                wrapperClass="loader"
                                type="Oval"
                                color="#11cdef"
                                height={19}
                                width={19}
                              />)
                              : (<i className="fas fa-download"></i>)
                          }</span>
                        <span className="btn-inner--text">Load</span>
                      </button>
                    </div>
                    {
                      !this.props.radarr.arePathsValid ? (
                        <Alert className="mt-3 mb-0 text-wrap " color="warning">
                          <strong>Could not find any paths.</strong>
                        </Alert>)
                        : null
                    }
                    {
                      this.props.isSubmitted && this.props.radarr.paths.length === 0 ? (
                        <Alert className="mt-3 mb-0 text-wrap " color="warning">
                          <strong>A path is required.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                  <Col lg="6" className="mb-4">
                    <div className="input-group-button">
                      <Dropdown
                        name="Profile"
                        value={this.props.category.profileId}
                        items={this.props.radarr.profiles.map(x => { return { name: x.name, value: x.id } })}
                        onChange={newProfile => this.setCategory("profileId", newProfile)} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadProfiles(true)} disabled={!this.props.canConnect} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.props.radarr.isLoadingProfiles ? (
                              <Oval
                                wrapperClass="loader"
                                type="Oval"
                                color="#11cdef"
                                height={19}
                                width={19}
                              />)
                              : (<i className="fas fa-download"></i>)
                          }</span>
                        <span className="btn-inner--text">Load</span>
                      </button>
                    </div>
                    {
                      !this.props.radarr.areProfilesValid ? (
                        <Alert className="mt-3 mb-0 text-wrap " color="warning">
                          <strong>Could not find any profiles.</strong>
                        </Alert>)
                        : null
                    }
                    {
                      this.props.isSubmitted && this.props.radarr.profiles.length === 0 ? (
                        <Alert className="mt-3 mb-0 text-wrap " color="warning">
                          <strong>A profile is required.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                </Row>
                <Row>
                  <Col lg="6">
                    <Dropdown
                      name="Min Availability"
                      value={this.props.category.minimumAvailability}
                      items={[{ name: "Announced", value: "announced" }, { name: "In Cinemas", value: "inCinemas" }, { name: "Released", value: "released" }, { name: "PreDB", value: "preDB" }]}
                      onChange={newMinimumAvailability => this.setCategory("minimumAvailability", newMinimumAvailability)} />
                  </Col>
                  {
                    this.props.apiVersion !== "2"
                      ? <>
                        <Col lg="6">
                          <div className="input-group-button mb-4">
                            <MultiDropdown
                              name="Tags"
                              placeholder=""
                              labelField="name"
                              valueField="id"
                              ignoreEmptyItems={true}
                              selectedItems={this.props.radarr.tags.filter(x => this.props.category.tags.includes(x.id))}
                              items={this.props.radarr.tags}
                              onChange={newTags => this.setCategory("tags", newTags.map(x => x.id))} />
                            <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadTags(true)} disabled={!this.props.canConnect} type="button">
                              <span className="btn-inner--icon">
                                {
                                  this.props.radarr.isLoadingTags ? (
                                    <Oval
                                      wrapperClass="loader"
                                      type="Oval"
                                      color="#11cdef"
                                      height={19}
                                      width={19}
                                    />)
                                    : (<i className="fas fa-download"></i>)
                                }</span>
                              <span className="btn-inner--text">Load</span>
                            </button>
                          </div>
                          {
                            !this.props.radarr.areTagsValid ? (
                              <Alert className="mt-3 mb-4 text-wrap " color="warning">
                                <strong>Could not load tags, cannot reach Radarr.</strong>
                              </Alert>)
                              : null
                          }
                        </Col>
                      </>
                      : null
                  }
                </Row>
                {
                  this.props.radarr.categories.length > 1
                    ? <Row>
                      <Col lg="12" className="text-right">
                        <button onClick={() => this.deleteCategory()} className="btn btn-icon btn-3 btn-danger" type="button">
                          <span className="btn-inner--icon"><i class="fas fa-trash"></i></span>
                          <span className="btn-inner--text">Remove</span>
                        </button>
                      </Col>
                    </Row>
                    : null
                }
              </div>
            </Collapse>
          </td>
        </tr>
      </>
    );
  }
}

const mapPropsToState = state => {
  return {
    radarr: state.movies.radarr
  }
};

const mapPropsToAction = {
  loadProfiles: loadRadarrProfiles,
  loadRootPaths: loadRadarrRootPaths,
  loadTags: loadRadarrTags,
  setRadarrCategory: setRadarrCategory,
  removeRadarrCategory: removeRadarrCategory,
};

export default connect(mapPropsToState, mapPropsToAction)(RadarrCategory);