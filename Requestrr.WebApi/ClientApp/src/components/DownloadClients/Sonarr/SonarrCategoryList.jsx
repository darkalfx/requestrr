import React from "react";
import { connect } from 'react-redux';
import { addSonarrCategory } from "../../../store/actions/SonarrClientActions"
import { setSonarrCategories } from "../../../store/actions/SonarrClientActions"
import SonarrCategory from "./SonarrCategory";

// reactstrap components
import {
  Button,
  Card,
  CardHeader,
  CardBody,
  FormGroup,
  Form,
  Input,
  Container,
  Row,
  Col,
  UncontrolledTooltip,
} from "reactstrap";

class SonarrCategoryList extends React.Component {
  constructor(props) {
    super(props);
    this.createSonarrCategory = this.createSonarrCategory.bind(this);
  }

  createSonarrCategory() {
    var newId = Math.floor((Math.random() * 900) + 1);

    while (this.props.categories.map(x => x.id).includes(newId)) {
      newId = Math.floor((Math.random() * 900) + 1);
    }

    var newCategory = {
      id: newId,
      name: "new-category",
      profileId: -1,
      rootFolder: "",
      useSeasonFolders: true,
      seriesType: "standard",
      languageId: -1,
      tags: [],
      wasCreated: true
    };

    this.props.addSonarrCategory(newCategory);
  }

  render() {
    return (
      <>
        <hr className="my-4" />
        <h6 className="heading-small text-muted">
          Sonarr Category Settings
        </h6>
        <div class="table-responsive mt-4 overflow-visible">
          <div>
            <table class="table align-items-center">
              <thead class="thead-dark">
                <tr>
                  <th scope="col" class="sort" data-sort="name">Category</th>
                  <th scope="col" class="text-right">Actions</th>
                </tr>
              </thead>
              <tbody class="list">
                {this.props.categories.map((category, key) => {
                  return (
                    <React.Fragment key={category.id}>
                      <SonarrCategory key={category.id} isSubmitted={this.props.isSubmitted} isSaving={this.props.isSaving} canConnect={this.props.canConnect} apiVersion={this.props.apiVersion} category={{ ...category }} />
                    </React.Fragment>)
                })}
                <tr>
                  <td className="text-right" colSpan="2">
                    <FormGroup className="form-group text-right mt-2">
                      <button onClick={this.createSonarrCategory} className="btn btn-icon btn-3 btn-success" type="button">
                        <span className="btn-inner--icon"><i className="fas fa-plus"></i></span>
                        <span className="btn-inner--text">Add new category</span>
                      </button>
                    </FormGroup>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <hr className="my-4" />
      </>
    );
  }
}

const mapPropsToState = state => {
  return {
    categories: state.tvShows.sonarr.categories
  }
};

const mapPropsToAction = {
  addSonarrCategory: addSonarrCategory,
  updateCategories: setSonarrCategories
};

export default connect(mapPropsToState, mapPropsToAction)(SonarrCategoryList);