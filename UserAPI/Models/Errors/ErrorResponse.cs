﻿using System.Collections.Generic;

namespace UserAPI.Models.Errors
{
    public class ErrorResponse
    {
        public List<ErrorModel> Errors { get; set; } = new List<ErrorModel>();
    }
}