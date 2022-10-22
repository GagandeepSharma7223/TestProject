"use strict";

// Class definition
var Folders = function () {
    // Private methods
    var getFilesData = function (rootDir = '') {
        $.ajax({
            type: "GET",
            url: '/api/folders/GetFiles?rootDir=' + rootDir,
            success: function (response) {
                bindFilesData(response);
            },
            error: function (err) {
                alert(err.statusText);
            }
        });
    }

    var getFolders = function (rootDir = '', query = '') {
        $.ajax({
            type: "GET",
            url: `/api/folders?rootDir=${rootDir}&searchText=${query}`,
            success: function (response) {
                bindFoldersData(response);
            },
            error: function (err) {
                alert(err.statusText);
            }
        });
    }

    var uploadFile = function (ele) {
        if (window.FormData !== undefined) {
            var fileName = $(ele).val().split("\\").pop();
            $(ele).siblings(".custom-file-label").addClass("selected").html(fileName);
            var fileUpload = $("#customFile").get(0);
            var files = fileUpload.files;

            // Create FormData object
            var fileData = new FormData();

            // Looping over all files and add it to FormData object
            for (var i = 0; i < files.length; i++) {
                fileData.append(files[i].name, files[i]);
            }

            fileData.append('DirectoryPath', $('#hdnDirPath').val());
            $.ajax({
                url: '/api/Folders/Upload',
                type: "POST",
                contentType: false,
                processData: false,
                data: fileData,
                success: function (result) {
                    const rootPath = $('#hdnDirPath').val();
                    $('.custom-file-label').text('Choose File');
                    getFilesData(rootPath);
                },
                error: function (err) {
                    alert(err.statusText);
                }
            });
        } else {
            alert("FormData is not supported.");
        }
    }

    var searchFile = function () {
        var query = $('#searchFile').val();
        if (query) {
            $.ajax({
                type: "GET",
                url: '/api/folders/searchFile?query=' + query,
                success: function (response) {
                    bindFilesData(response);
                    $('#btnClear').show();
                },
                error: function (err) {
                    alert(err.statusText);
                }
            });
        }
    }

    var downloadFile = function (filePath) {
        $.ajax({
            type: "Post",
            url: '/api/folders/download?filePath=' + filePath,
            xhrFields: {
                responseType: 'blob' // to avoid binary data being mangled on charset conversion
            },
            success: function (data, status, xmlHeaderRequest) {
                var downloadLink = document.createElement('a');
                var blob = new Blob([data],
                    {
                        type: xmlHeaderRequest.getResponseHeader('Content-Type')
                    });
                var url = window.URL || window.webkitURL;
                var downloadUrl = url.createObjectURL(blob);
                var fileName = '';

                // get the file name from the content disposition
                var disposition = xmlHeaderRequest.getResponseHeader('Content-Disposition');
                if (disposition && disposition.indexOf('attachment') !== -1) {
                    var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                    var matches = filenameRegex.exec(disposition);
                    if (matches != null && matches[1]) {
                        fileName = matches[1].replace(/['"]/g, '');
                    }
                }

                if (typeof window.navigator.msSaveBlob !== 'undefined') {
                    // IE workaround for "HTML7007" and "Access Denied" error.
                    window.navigator.msSaveBlob(blob, fileName);
                } else {
                    if (fileName) {
                        if (typeof downloadLink.download === 'undefined') {
                            window.location = downloadUrl;
                        } else {
                            downloadLink.href = downloadUrl;
                            downloadLink.download = fileName;
                            document.body.appendChild(downloadLink);
                            downloadLink.click();
                        }
                    } else {
                        window.location = downloadUrl;
                    }

                    setTimeout(function () {
                        url.revokeObjectURL(downloadUrl);
                    },
                        100);
                }
            },
            error: function (err) {
                alert(err.statusText);
            }
        });
    }

    var bindFilesData = function (response) {
        var fileHTML = '';
        if (response.success && response.files.length) {
            // Append HTML to container.
            fileHTML = `<div class="col-md-12"><h5>Files (${response.files.length})</h5></div>`
            response.files.forEach(function (file) {
                fileHTML += `
                            <div class="col-md-4 text-center">
                                <a class="btn icon-file">
                                    <i class="fa fa-file fa-2x" aria-hidden="true"></i>
                                    <i class="fa fa-cloud-download download text-success" data-path="${file.path}" data-file="${file.fileName}" aria-hidden="true"></i>
                                </a>
                                <p>${file.fileName} (${file.readableFileSize})</p>
                            </div>`
            });
        }
        else {
            fileHTML = '<div class="col-md-12 text-center">No Data Found</p>'
        }
        $('#filesContainer').empty();
        $('#filesContainer').append(fileHTML);
    }

    var bindFoldersData = function (response) {
        var folderHTML = `<div class="col-md-12"><h5>Folders (${response.length}) <a class='btn' id="btnAddFolder"><i class="fa fa-plus" aria-hidden="true"></i></a></h5></div>`;
        if (response.length) {
            // Append HTML to container.
            response.forEach(function (folder) {
                folderHTML += `
                            <div class="col-md-4 text-center">
                                <a class="btn icon-file folder-icon" data-folder=${folder.path}>
                                    <i class="fa fa-folder fa-2x" aria-hidden="true"></i>
                                </a>`;
                if (folder.directorySize > 0) {
                    folderHTML += `<p>${folder.name} (${folder.readableDirectorySize})</p></div>`;
                }
                else {
                    folderHTML += `<p>${folder.name}</p></div>`;
                }
            });
        }
        $('#foldersContainer').empty();
        $('#foldersContainer').append(folderHTML);
    }

    var saveFolder = function () {
        const folderName = $('#folder-name').val();
        const dirPath = $('#hdnDirPath').val();
        if (folderName) {
            $.ajax({
                type: "POST",
                url: '/api/Folders/AddDirectory',
                contentType: 'application/json',
                data: JSON.stringify({ folderName: folderName, rootDirectory: dirPath }),
                success: function (response) {
                    if (response.success) {
                        $('#myModal').modal('hide');
                        $('#myModal input').val('');
                        getFolders(dirPath);
                    }
                    else {
                        // show error message alert
                        alert(response.errorMessage);
                    }
                },
                error: function (err) {
                    alert(err.statusText);
                }
            });
        }
    }

    return {
        getFiles: function (rootDir) {
            getFilesData(rootDir);
        },

        get: function (rootDir) {
            getFolders(rootDir);
        },

        back: function () {
            const rootPath = $('#hdnRootPath').val();
            const dirPath = $('#hdnDirPath').val();
            if (rootPath.toLowerCase() == dirPath.toLowerCase()) {
                getFolders();
                getFilesData();
            }
            else {
                // Remove folder from end of path
                const path = dirPath.substring(0, dirPath.lastIndexOf('\\'));
                $('#hdnDirPath').val(path);
                // hide back button
                if (rootPath.toLowerCase() == path.toLowerCase()) {
                    $('#btnBack').hide();
                    $('#hdnDirPath').val('');
                }
                getFolders(path);
                getFilesData(path);
            }
        },

        clear: function () {
            $('#btnClear').hide();
            $('#btnBack').hide();
            $('#searchFile').val('');
            $('#hdnDirPath').val('');
            getFilesData();
            getFolders();
        },

        upload: function (ele) {
            uploadFile(ele);
        },

        search: function () {
            var query = $('#searchFile').val();
            searchFile();
            getFolders('', query);
        },

        download: function (filePath) {
            downloadFile(filePath);
        },

        saveFolder: function () {
            saveFolder();
        },

        onKeyPressSearchFile: function (e) {
            var key = e.which;
            if (key == 13)  // the enter key code
            {
                var query = $('#searchFile').val();
                searchFile();
                getFolders('', query);
                return false;
            }
        }
    };
}();
