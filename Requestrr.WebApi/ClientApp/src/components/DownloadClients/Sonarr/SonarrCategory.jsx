import React from "react";
import Loader from 'react-loader-spinner'
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { loadSonarrProfiles } from "../../../store/actions/SonarrClientActions"
import { loadSonarrRootPaths } from "../../../store/actions/SonarrClientActions"
import { loadSonarrTags } from "../../../store/actions/SonarrClientActions"
import { loadSonarrLanguages } from "../../../store/actions/SonarrClientActions"
import { setSonarrCategory } from "../../../store/actions/SonarrClientActions"
import { removeSonarrCategory } from "../../../store/actions/SonarrClientActions"
import ValidatedTextbox from "../../Inputs/ValidatedTextbox"
import Dropdown from "../../Inputs/Dropdown"
import MultiDropdown from "../../Inputs/MultiDropdown"

import {
  FormGroup,
  Input,
  Row,
  Col,
  Collapse
} from "reactstrap";

class SonarrCategory extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      arePathsValid: true,
      areProfilesValid: true,
      areTagsValid: true,
      areLanguagesValid: true,
      nameErrorMessage: "",
      isNameValid: true,
      isOpen: false,
    };

    this.validateName = this.validateName.bind(this);
    this.setPaths = this.setPaths.bind(this);
    this.setProfiles = this.setProfiles.bind(this);
    this.setTags = this.setTags.bind(this);
    this.setLanguages = this.setLanguages.bind(this);
    this.onUseSeasonFoldersChanged = this.onUseSeasonFoldersChanged.bind(this);
    this.setCategory = this.setCategory.bind(this);
    this.deleteCategory = this.deleteCategory.bind(this);
  }

  componentDidMount() {
    var category = { ...this.props.category };

    if (category.wasCreated) {
      this.setState({ isOpen: true });
    }

    if (this.props.canConnect) {
      this.props.loadProfiles(false);
      this.props.loadRootPaths(false);
      this.props.loadTags(false);
      this.props.loadLanguages(false);
    }

    category = this.setPaths(category);
    category = this.setProfiles(category);
    category = this.setTags(category);
    category = this.setLanguages(category);

    this.setCategory(category)
  }

  componentDidUpdate(prevProps, prevState) {
    var category = { ...this.props.category };

    var previousNames = prevProps.sonarr.categories.map(x => x.name);
    var currentNames = this.props.sonarr.categories.map(x => x.name);

    if (!(previousNames.length == currentNames.length && currentNames.every((value, index) => previousNames[index] == value))) {
      this.validateName(category.name)
    }

    if (this.props.canConnect) {
      this.props.loadProfiles(false);
      this.props.loadRootPaths(false);
      this.props.loadTags(false);
      this.props.loadLanguages(false);
    }

    if (!(prevProps.sonarr.tags.length == this.props.sonarr.tags.length && prevProps.sonarr.tags.reduce((a, b, i) => a && this.props.sonarr.tags[i], true))) {
      category = this.setTags(category);
    }

    if (!(prevProps.sonarr.profiles.length == this.props.sonarr.profiles.length && prevProps.sonarr.profiles.reduce((a, b, i) => a && this.props.sonarr.profiles[i], true))) {
      category = this.setProfiles(category);
    }

    if (!(prevProps.sonarr.paths.length == this.props.sonarr.paths.length && prevProps.sonarr.paths.reduce((a, b, i) => a && this.props.sonarr.paths[i], true))) {
      category = this.setPaths(category);
    }

    if (!(prevProps.sonarr.languages.length == this.props.sonarr.languages.length && prevProps.sonarr.languages.reduce((a, b, i) => a && this.props.sonarr.languages[i], true))) {
      category = this.setLanguages(category);
    }

    if (JSON.stringify(category) !== JSON.stringify(this.props.category)) {
      this.setCategory(category)
    }

    if (prevProps.isSaving != this.props.isSaving) {
      this.setState({
        isOpen: false,
      });
    }
  }

  onUseSeasonFoldersChanged = event => {
    this.setCategory({ ...this.props.category, useSeasonFolders: !this.props.category.useSeasonFolders })
  }

  setPaths(category) {
    if (this.props.sonarr.paths.length > 0) {
      var defaultPathId = this.props.sonarr.paths[0].path;
      var pathValue = this.props.sonarr.paths.map(x => x.path).includes(category.rootFolder) ? category.rootFolder : defaultPathId;

      category = { ...category, rootFolder: pathValue };
    }

    return category;
  }

  setProfiles(category) {
    if (this.props.sonarr.profiles.length > 0) {
      var defaultProfileId = this.props.sonarr.profiles[0].id;
      var profileValue = this.props.sonarr.profiles.map(x => x.id).includes(category.profileId) ? category.profileId : defaultProfileId;

      category = { ...category, profileId: profileValue };
    }

    return category;
  }

  setTags(category) {
    if (this.props.sonarr.tags.length > 0) {
      var tagsValue = category.tags.filter(x => this.props.sonarr.tags.map(x => x.id).includes(x));

      category = { ...category, tags: tagsValue };
    }

    return category;
  }

  setLanguages(category) {
    if (this.props.sonarr.languages.length > 0) {
      var defaultLanguageId = this.props.sonarr.languages[0].id;
      var languageValue = this.props.sonarr.languages.map(x => x.id).includes(category.languageId) ? category.languageId : defaultLanguageId;

      category = { ...category, languageId: languageValue };
    }

    return category;
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
      if (this.props.sonarr.categories.map(x => x.id).includes(this.props.category.id) && this.props.sonarr.categories.filter(c => typeof c.id !== 'undefined' && c.id != this.props.category.id && c.name.toLowerCase().trim() == value.toLowerCase().trim()).length > 0) {
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

  setCategory(category) {
    this.props.setSonarrCategory(category);
  }

  deleteCategory() {
    this.setState({
      isOpen: false,
    }, () => setTimeout(() => this.props.removeSonarrCategory(this.props.category.id), 150));
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
            <button onClick={() => this.setState({ isOpen: !this.state.isOpen })} disabled={this.state.isOpen && (!this.state.isNameValid || !this.props.sonarr.areLanguagesValid || !this.props.sonarr.arePathsValid || !this.props.sonarr.areProfilesValid || !this.props.sonarr.areTagsValid)} className="btn btn-icon btn-3 btn-info" type="button">
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
                      onChange={newName => this.setCategory({ ...this.props.category, name: newName })}
                      onValidate={isValid => this.setState({ isNameValid: isValid })} />
                  </Col>
                </Row>
                <Row>
                  <Col lg="6" className="mb-4">
                    <div className="input-group-button">
                      <Dropdown
                        name="Path"
                        value={this.props.category.rootFolder}
                        items={this.props.sonarr.paths.map(x => { return { name: x.path, value: x.path } })}
                        onChange={newPath => this.setCategory({ ...this.props.category, rootFolder: newPath })} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadRootPaths(true)} disabled={!this.props.canConnect} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.props.sonarr.isLoadingPaths ? (
                              <Loader
                                className="loader"
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
                      !this.props.sonarr.arePathsValid ? (
                        <Alert className="mt-3 mb-0 text-wrap " color="warning">
                          <strong>Could not load paths, cannot reach Sonarr.</strong>
                        </Alert>)
                        : null
                    }
                    {
                      this.props.isSubmitted && this.props.sonarr.paths.length === 0 ? (
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
                        items={this.props.sonarr.profiles.map(x => { return { name: x.name, value: x.id } })}
                        onChange={newProfile => this.setCategory({ ...this.props.category, profileId: newProfile })} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadProfiles(true)} disabled={!this.props.canConnect} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.props.sonarr.isLoadingProfiles ? (
                              <Loader
                                className="loader"
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
                      !this.props.sonarr.areProfilesValid ? (
                        <Alert className="mt-3 mb-0 text-wrap " color="warning">
                          <strong>Could not load profiles, cannot reach Sonarr.</strong>
                        </Alert>)
                        : null
                    }
                    {
                      this.props.isSubmitted && this.props.sonarr.profiles.length === 0 ? (
                        <Alert className="mt-3 mb-0 text-wrap " color="warning">
                          <strong>A profile is required.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                </Row>
                {
                  this.props.apiVersion !== "2"
                    ? <>
                      <Row>
                        <Col lg="6">
                          <div className="input-group-button mb-4">
                            <Dropdown
                              name="Language"
                              value={this.props.category.languageId}
                              items={this.props.sonarr.languages.map(x => { return { name: x.name, value: x.id } })}
                              onChange={newLanguage => this.setCategory({ ...this.props.category, languageId: newLanguage })} />
                            <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadLanguages(true)} disabled={!this.props.canConnect} type="button">
                              <span className="btn-inner--icon">
                                {
                                  this.props.sonarr.isLoadingLanguages ? (
                                    <Loader
                                      className="loader"
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
                            !this.props.sonarr.areLanguagesValid ? (
                              <Alert className="mt-3 mb-4 text-wrap " color="warning">
                                <strong>Could not load languages, cannot reach Sonarr.</strong>
                              </Alert>)
                              : null
                          }
                          {
                            this.props.isSubmitted && this.props.sonarr.languages.length === 0 ? (
                              <Alert className="mt-3 mb-4 text-wrap " color="warning">
                                <strong>A language is required.</strong>
                              </Alert>)
                              : null
                          }
                        </Col>
                        <Col lg="6">
                          <div className="input-group-button mb-4">
                            <MultiDropdown
                              name="Tags"
                              placeholder=""
                              labelField="name"
                              valueField="id"
                              ignoreEmptyItems={true}
                              selectedItems={this.props.sonarr.tags.filter(x => this.props.category.tags.includes(x.id))}
                              items={this.props.sonarr.tags}
                              onChange={newTags => this.setCategory({ ...this.props.category, tags: newTags.map(x => x.id) })} />
                            <button className="btn btn-icon btn-3 btn-default" onClick={() => this.props.loadTags(true)} disabled={!this.props.canConnect} type="button">
                              <span className="btn-inner--icon">
                                {
                                  this.props.sonarr.isLoadingTags ? (
                                    <Loader
                                      className="loader"
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
                            !this.props.sonarr.areTagsValid ? (
                              <Alert className="mt-3 mb-4 text-wrap " color="warning">
                                <strong>Could not load tags, cannot reach Sonarr.</strong>
                              </Alert>)
                              : null
                          }
                        </Col>
                      </Row>
                    </>
                    : null
                }
                <Row>
                  <Col lg="6">
                    <Dropdown
                      name="Series Type"
                      value={this.props.category.seriesType}
                      items={[{ name: "Standard", value: "standard" }, { name: "Daily", value: "daily" }, { name: "Anime", value: "anime" }]}
                      onChange={newSeriesType => this.setCategory({ ...this.props.category, seriesType: newSeriesType })} />
                  </Col>
                </Row>
                <Row>
                  <Col lg="6">
                    <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                      <Input
                        className="custom-control-input"
                        id={"tvUseSeasonFolders" + this.props.category.id}
                        type="checkbox"
                        onChange={this.onUseSeasonFoldersChanged}
                        checked={this.props.category.useSeasonFolders}
                      />
                      <label
                        className="custom-control-label"
                        htmlFor={"tvUseSeasonFolders" + this.props.category.id}>
                        <span className="text-muted">Use season folders</span>
                      </label>
                    </FormGroup>
                  </Col>
                </Row>
                {
                  this.props.sonarr.categories.length > 1
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
    sonarr: state.tvShows.sonarr
  }
};

const mapPropsToAction = {
  loadProfiles: loadSonarrProfiles,
  loadRootPaths: loadSonarrRootPaths,
  loadTags: loadSonarrTags,
  loadLanguages: loadSonarrLanguages,
  setSonarrCategory: setSonarrCategory,
  removeSonarrCategory: removeSonarrCategory,
};

export default connect(mapPropsToState, mapPropsToAction)(SonarrCategory);