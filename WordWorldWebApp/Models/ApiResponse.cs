using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Exceptions;
using WordWorldWebApp.Utils;

namespace WordWorldWebApp.Models
{
    public class ApiResponse
    {        
        public string Status { get; set; }

        public object Data { get; set; }

        public static ApiResponse Success(object data)
        {
            return new ApiResponse()
            {
                Status = "ok",
                Data = data
            };
        }

        public static ApiResponse Fail(string error, object data = null)
        {
            return new ApiResponse()
            {
                Status = error,
                Data = data
            };
        }

        public static ApiResponse Fail(Exception exception)
        {
            string code = exception switch
            {
                InvalidPlacementException => "invalid_placement",
                LetterNotInInventoryException => "letter_not_in_inventory",
                PlayerNotFoundException => "player_not_found",
                UnknownWordException => "unknown_word",
                WordTooShortException => "word_too_short",
                IndexOutOfRangeException => "out_of_range",
                ActionArgumentException => "invalid_arguments",
                AmbiguousJokerException => "ambiguous_joker",

                _ => null
            };

            if (code == null)
            {
                return null;
            }

            return Fail(code, (exception as IProvideErrorData)?.GetErrorData());
        }
    }
}
