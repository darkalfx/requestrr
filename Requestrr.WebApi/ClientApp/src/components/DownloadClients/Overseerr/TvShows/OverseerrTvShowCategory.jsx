import React from "react";
import { Oval } from 'react-loader-spinner'
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { loadSonarrServiceSettings } from "../../../../store/actions/OverseerrClientSonarrActions"
import { setOverseerrTvShowCategory } from "../../../../store/actions/OverseerrClientSonarrActions"
import { removeOverseerrTvShowCategory } from "../../../../store/actions/OverseerrClientSonarrActions"
import ValidatedTextbox from "../../../Inputs/ValidatedTextbox"
import Dropdown from "../../../Inputs/Dropdown"
import MultiDropdown from "../../../Inputs/MultiDropdown"

import {
  FormGroup,
  Input,
  Row,
  Col,
  Collapse
} from "reactstrap";

class OverseerrTvShowCategory extends React.Component {
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
      this.props.loadServiceSettings(false);
    }
  }

  componentDidUpdate(prevProps, prevState) {
    var previousNames = prevProps.overseerr.categories.map(x => x.name);
    var currentNames = this.props.overseerr.categories.map(x => x.name);

    if (!(previousNames.length === currentNames.length && currentNames.every((value, index) => previousNames[index] === value))) {
      this.validateName(this.props.category.name)
    }

    if (this.props.canConnect) {
      this.props.loadServiceSettings(false);
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
      if (this.props.overseerr.categories.map(x => x.id).includes(this.props.category.id) && this.props.overseerr.categories.filter(c => typeof c.id !== 'undefined' && c.id !== this.props.category.id && c.name.toLowerCase().trim() === value.toLowerCase().trim()).length > 0) {
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
    this.props.setOverseerrCategory(this.props.category.id, fieldChanged, data);
  }

  deleteCategory() {
    this.setState({
      isOpen: false,
    }, () => setTimeout(() => this.props.removeOverseerrCategory(this.props.category.id), 150));
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
            <button onClick={() => this.setState({ isOpen: !this.state.isOpen })} disabled={this.state.isOpen && (!this.state.isNameValid || !this.props.overseerr.isSonarrServiceSettingsValid)} className="btn btn-icon btn-3 btn-info" type="button">
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
                  <Col lg="6" className="mb-4">
                    <div className="input-group-button">
                      <Dropdown
                        name="Sonarr Instance"
                        value={this.props.category.serviceId}
                        items={this.props.overseerr.sonarrServiceSettings.sonarrServices.map(x => { return { name: x.name, value: x.id } })}
                        onChange={newServiceId => this.setCategory("serviceId", newServiceId)} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadServiceSettings(true)} disabled={!this.props.canConnect} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.props.overseerr.isLoadinSonarrServiceSettings ? (
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
                      this.props.overseerr.sonarrServiceSettings.sonarrServices.length === 0 ? (
                        <Alert className="mt-3 mb-0 text-wrap " color="warning">
                          <strong>Could not find any sonarr instances.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                </Row>
                <Row>
                  <Col lg="6" className="mb-4">
                    <div className="input-group-button">
                      <Dropdown
                        name="Path"
                        value={this.props.category.rootFolder}
                        items={this.props.overseerr.sonarrServiceSettings.sonarrServices.some(x => x.id === this.props.category.serviceId) ? this.props.overseerr.sonarrServiceSettings.sonarrServices.filter(x => x.id === this.props.category.serviceId)[0].rootPaths.map(x => { return { name: x.name, value: x.name } }) : []}
                        onChange={newRootFolder => this.setCategory("rootFolder", newRootFolder)} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadServiceSettings(true)} disabled={!this.props.canConnect} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.props.overseerr.isLoadinSonarrServiceSettings ? (
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
                      (this.props.overseerr.sonarrServiceSettings.sonarrServices.some(x => x.id === this.props.category.serviceId) ? this.props.overseerr.sonarrServiceSettings.sonarrServices.filter(x => x.id === this.props.category.serviceId)[0].rootPaths.map(x => { return { name: x.name, value: x.name } }) : []).length === 0 ? (
                        <Alert className="mt-3 mb-0 text-wrap " color="warning">
                          <strong>Could not find any paths.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                  <Col lg="6" className="mb-4">
                    <div className="input-group-button">
                      <Dropdown
                        name="Profile"
                        value={this.props.category.profileId}
                        items={this.props.overseerr.sonarrServiceSettings.sonarrServices.some(x => x.id === this.props.category.serviceId) ? this.props.overseerr.sonarrServiceSettings.sonarrServices.filter(x => x.id === this.props.category.serviceId)[0].profiles.map(x => { return { name: x.name, value: x.id } }) : []}
                        onChange={newProfileId => this.setCategory("profileId", newProfileId)} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadServiceSettings(true)} disabled={!this.props.canConnect} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.props.overseerr.isLoadinSonarrServiceSettings ? (
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
                      (this.props.overseerr.sonarrServiceSettings.sonarrServices.some(x => x.id === this.props.category.serviceId) ? this.props.overseerr.sonarrServiceSettings.sonarrServices.filter(x => x.id === this.props.category.serviceId)[0].profiles.map(x => { return { name: x.name, value: x.id } }) : []).length === 0 ? (
                        <Alert className="mt-3 mb-0 text-wrap " color="warning">
                          <strong>Could not find any profiles.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                </Row>
                <Row>
                  <Col lg="6" className="mb-4">
                    <div className="input-group-button">
                      <Dropdown
                        name="Language"
                        value={this.props.category.languageProfileId}
                        items={this.props.overseerr.sonarrServiceSettings.sonarrServices.some(x => x.id === this.props.category.serviceId) ? this.props.overseerr.sonarrServiceSettings.sonarrServices.filter(x => x.id === this.props.category.serviceId)[0].languageProfiles.map(x => { return { name: x.name, value: x.id } }) : []}
                        onChange={newLanguageId => this.setCategory("languageProfileId", newLanguageId)} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadServiceSettings(true)} disabled={!this.props.canConnect} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.props.overseerr.isLoadinSonarrServiceSettings ? (
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
                      (this.props.overseerr.sonarrServiceSettings.sonarrServices.some(x => x.id === this.props.category.serviceId) ? this.props.overseerr.sonarrServiceSettings.sonarrServices.filter(x => x.id === this.props.category.serviceId)[0].languageProfiles.map(x => { return { name: x.name, value: x.id } }) : []).length === 0 ? (
                        <Alert className="mt-3 mb-0 text-wrap " color="warning">
                          <strong>Could not find any languages.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                  <Col lg="6">
                    <div className="input-group-button mb-4">
                      <MultiDropdown
                        name="Tags"
                        placeholder=""
                        selectedItems={(this.props.overseerr.sonarrServiceSettings.sonarrServices.some(x => x.id === this.props.category.serviceId) ? this.props.overseerr.sonarrServiceSettings.sonarrServices.filter(x => x.id === this.props.category.serviceId)[0].tags : []).filter(x => this.props.category.tags.includes(x.id))}
                        items={this.props.overseerr.sonarrServiceSettings.sonarrServices.some(x => x.id === this.props.category.serviceId) ? this.props.overseerr.sonarrServiceSettings.sonarrServices.filter(x => x.id === this.props.category.serviceId)[0].tags : []}
                        onChange={newTags => this.setCategory("tags", newTags.map(x => x.id))} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadServiceSettings(true)} disabled={!this.props.canConnect} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.props.overseerr.isLoadinSonarrServiceSettings ? (
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
                      !this.props.overseerr.isSonarrServiceSettingsValid ? (
                        <Alert className="mt-3 mb-4 text-wrap " color="warning">
                          <strong>Could not load tags, cannot reach Overseerr.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                </Row>
                <Row>
                  <Col lg="6">
                    <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                      <Input
                        className="custom-control-input"
                        id={"is4K" + this.props.category.id}
                        type="checkbox"
                        onChange={() => this.setCategory("is4K", !this.props.category.is4K)}
                        checked={this.props.category.is4K}
                      />
                      <label
                        className="custom-control-label"
                        htmlFor={"is4K" + this.props.category.id}>
                        <span className="text-muted">Is 4K Server</span>
                      </label>
                    </FormGroup>
                  </Col>
                </Row>
                <Row>
                  <Col lg="12" className="text-right">
                    <button onClick={() => this.deleteCategory()} className="btn btn-icon btn-3 btn-danger" type="button">
                      <span className="btn-inner--icon"><i class="fas fa-trash"></i></span>
                      <span className="btn-inner--text">Remove</span>
                    </button>
                  </Col>
                </Row>
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
    overseerr: state.tvShows.overseerr
  }
};

const mapPropsToAction = {
  loadServiceSettings: loadSonarrServiceSettings,
  setOverseerrCategory: setOverseerrTvShowCategory,
  removeOverseerrCategory: removeOverseerrTvShowCategory,
};

export default connect(mapPropsToState, mapPropsToAction)(OverseerrTvShowCategory);