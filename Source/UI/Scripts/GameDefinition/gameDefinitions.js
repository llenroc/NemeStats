﻿//Usings
Namespace("Views.GameDefinition");

//Initialization
Views.GameDefinition.GameDefinitions = function () {
    this.$container = null;
    this.$gameDefinitionsTable = null;
};

//Implementation
Views.GameDefinition.GameDefinitions.prototype = {
    init: function () {
        this.$container = $(".gameDefinitionsList");
        this.$gameDefinitionsTable = this.$container.find("table");
    },

    onGameCreated: function (game) {
        this.$gameDefinitionsTable.find("tr:last").after(
            "<tr> \
                <td> \
                    <a href='/GameDefinition/Details/" + game.Id + "'>" + game.Name + "</a> \
                </td> \
                <td>0</td>\
                <td> \
                    <a href='/GameDefinition/Edit/" + game.Id + "' title='Edit'> \
                        <i class='fa fa-pencil fa-3x'></i> \
                    </a> \
                </td> \
            </tr>");
    }
}