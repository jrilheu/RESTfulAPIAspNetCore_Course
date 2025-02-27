﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace Library.API.Helpers
{
    public class UnprocessableEntityObjectResult : ObjectResult
    {
        public UnprocessableEntityObjectResult(ModelStateDictionary modelState)
            :base(new SerializableError(modelState))
        {
            if (modelState == null)
            {
                throw new ArgumentException(nameof(modelState));
            }

            StatusCode = 422;
        }
    }
}
