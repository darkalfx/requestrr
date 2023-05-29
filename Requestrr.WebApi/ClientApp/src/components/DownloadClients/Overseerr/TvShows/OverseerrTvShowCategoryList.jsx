import React from "react";
import { connect } from 'react-redux';
import { addOverseerrTvShowCategory } from "../../../../store/actions/OverseerrClientSonarrActions"
import OverseerrTvShowCategory from "./OverseerrTvShowCategory";

// reactstrap components
import {
  FormGroup
} from "reactstrap";

class OverseerrTvShowCategoryList extends React.Component {
  constructor(props) {
    super(props);
    this.createOverseerrCategory = this.createOverseerrCategory.bind(this);
  }

  createOverseerrCategory() {
    var newId = Math.floor((Math.random() * 900) + 1);

    while (this.props.overseerr.categories.map(x => x.id).includes(newId)) {
      newId = Math.floor((Math.random() * 900) + 1);
    }

    var newCategory = {
      id: newId,
      name: "new-category",
      serviceId: this.props.overseerr.sonarrServiceSettings.sonarrServices.length > 0 ? this.props.overseerr.sonarrServiceSettings.sonarrServices[0].id : -1,
      profileId: -1,
      rootFolder: "",
      tags: [],
      wasCreated: true
    };

    this.props.addOverseerrCategory(newCategory);
  }

  render() {
    return (
      <>
        <hr className="my-4" />
        <h6 className="heading-small text-muted">
          Overseerr Category Settings
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
                {this.props.overseerr.categories.map((category, key) => {
                  return (
                    <React.Fragment key={category.id}>
                      <OverseerrTvShowCategory key={category.id} isSubmitted={this.props.isSubmitted} isSaving={this.props.isSaving} canConnect={this.props.canConnect} apiVersion={this.props.apiVersion} category={{ ...category }} />
                    </React.Fragment>)
                })}
                <tr>
                  <td className="text-right" colSpan="2">
                    <FormGroup className="form-group text-right mt-2">
                      <button onClick={this.createOverseerrCategory} className="btn btn-icon btn-3 btn-success" type="button">
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
    overseerr: state.tvShows.overseerr
  }
};

const mapPropsToAction = {
  addOverseerrCategory: addOverseerrTvShowCategory,
};

export default connect(mapPropsToState, mapPropsToAction)(OverseerrTvShowCategoryList);